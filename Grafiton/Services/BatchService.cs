using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public class BatchService : IBatchService
{
    private readonly IPdfEditService _pdfEditService;
    private readonly IConversionService _conversionService;
    private readonly ICompressionService _compressionService;

    public BatchService(
        IPdfEditService pdfEditService,
        IConversionService conversionService,
        ICompressionService compressionService)
    {
        _pdfEditService = pdfEditService;
        _conversionService = conversionService;
        _compressionService = compressionService;
    }

    public async Task ProcessBatchAsync(List<BatchJob> jobs, IProgress<double>? overallProgress = null, CancellationToken cancellationToken = default)
    {
        if (jobs == null || jobs.Count == 0) return;

        int total = jobs.Count;

        for (int i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var job = jobs[i];
            job.Status = BatchJobStatus.Processing;
            job.ProgressPercent = 10;

            try
            {
                string outDir = string.IsNullOrEmpty(job.OutputDirectory)
                    ? Path.GetDirectoryName(job.SourceFilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : job.OutputDirectory;

                string baseName = Path.GetFileNameWithoutExtension(job.SourceFilePath);

                switch (job.Operation)
                {
                    case BatchOperationType.Watermark:
                        string wmOut = Path.Combine(outDir, $"{baseName}_Filigrane.pdf");
                        var opts = new WatermarkOptions { Text = job.WatermarkText, Opacity = 0.3, RotationAngle = -45 };
                        await _pdfEditService.AddTextWatermarkAsync(job.SourceFilePath, opts, wmOut);
                        job.OutputFilePath = wmOut;
                        break;

                    case BatchOperationType.Compress:
                        string compOut = Path.Combine(outDir, $"{baseName}_Compresse.pdf");
                        var compRes = await _compressionService.CompressPdfAsync(job.SourceFilePath, CompressionQuality.Medium, compOut);
                        job.OutputFilePath = compRes.OutputFilePath;
                        break;

                    case BatchOperationType.ConvertToImages:
                        string imgOutDir = Path.Combine(outDir, $"{baseName}_Images");
                        var files = await _conversionService.ExportPdfToImagesAsync(job.SourceFilePath, "PNG", 150, imgOutDir, null, cancellationToken);
                        job.OutputFilePath = imgOutDir;
                        break;

                    case BatchOperationType.ConvertToDocx:
                        string docxOut = await _conversionService.ConvertDocumentWithLibreOfficeAsync(job.SourceFilePath, "docx", outDir, null, cancellationToken);
                        job.OutputFilePath = docxOut;
                        break;

                    case BatchOperationType.ProtectPassword:
                        string passOut = Path.Combine(outDir, $"{baseName}_Protege.pdf");
                        await _pdfEditService.ProtectPdfAsync(job.SourceFilePath, job.Password, job.Password, passOut);
                        job.OutputFilePath = passOut;
                        break;
                }

                job.Status = BatchJobStatus.Completed;
                job.ProgressPercent = 100;
            }
            catch (Exception ex)
            {
                job.Status = BatchJobStatus.Failed;
                job.ErrorMessage = ex.Message;
            }

            overallProgress?.Report((double)(i + 1) / total * 100.0);
        }
    }
}
