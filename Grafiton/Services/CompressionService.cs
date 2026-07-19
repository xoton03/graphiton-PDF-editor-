using System;
using System.IO;
using System.Threading.Tasks;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using Grafiton.Models;

namespace Grafiton.Services;

public class CompressionService : ICompressionService
{
    public Task<CompressionResult> CompressPdfAsync(string inputPath, CompressionQuality quality, string outputPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Le fichier source est introuvable.", inputPath);

            var origInfo = new FileInfo(inputPath);
            long origSize = origInfo.Length;

            using var doc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
            using var outputDoc = new PdfDocument();

            // Configure PDF options for maximum compression
            outputDoc.Options.CompressContentStreams = true;
            outputDoc.Options.NoCompression = false;

            for (int i = 0; i < doc.PageCount; i++)
            {
                outputDoc.AddPage(doc.Pages[i]);
            }

            outputDoc.Save(outputPath);

            var newInfo = new FileInfo(outputPath);
            long compressedSize = newInfo.Length;

            return new CompressionResult
            {
                OutputFilePath = outputPath,
                OriginalSizeBytes = origSize,
                CompressedSizeBytes = compressedSize
            };
        });
    }
}
