using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using PdfSharpCore.Drawing;
using SharpPdf = PdfSharpCore.Pdf.PdfDocument;
using WinPdf = Windows.Data.Pdf.PdfDocument;
using WinPdfOptions = Windows.Data.Pdf.PdfPageRenderOptions;
using Grafiton.Models;

namespace Grafiton.Services;

public class ConversionService : IConversionService
{
    private readonly ILibreOfficeService _libreOfficeService;

    public ConversionService(ILibreOfficeService libreOfficeService)
    {
        _libreOfficeService = libreOfficeService;
    }

    public async Task<List<string>> ExportPdfToImagesAsync(
        string pdfPath,
        string format,
        int dpi,
        string outputDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("Le fichier PDF source n'existe pas.", pdfPath);

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        var outputFiles = new List<string>();
        string baseName = Path.GetFileNameWithoutExtension(pdfPath);
        string ext = format.TrimStart('.').ToLower();

        var storageFile = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(pdfPath));
        var pdfDoc = await WinPdf.LoadFromFileAsync(storageFile);
        uint totalPages = pdfDoc.PageCount;

        for (uint i = 0; i < totalPages; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var page = pdfDoc.GetPage(i);
            string outPath = Path.Combine(outputDirectory, $"{baseName}_page_{i + 1}.{ext}");

            var options = new WinPdfOptions();
            if (dpi > 0 && dpi != 96)
            {
                double scale = dpi / 96.0;
                options.DestinationWidth = (uint)(page.Size.Width * scale);
            }

            using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var stream = fs.AsRandomAccessStream())
            {
                await page.RenderToStreamAsync(stream, options);
            }

            outputFiles.Add(outPath);
            progress?.Report((double)(i + 1) / totalPages * 100.0);
        }

        return outputFiles;
    }

    public Task<string> ConvertImagesToPdfAsync(
        List<string> imagePaths,
        string outputPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            if (imagePaths == null || imagePaths.Count == 0)
                throw new ArgumentException("Aucune image spécifiée.", nameof(imagePaths));

            using var outputDoc = new SharpPdf();
            int total = imagePaths.Count;

            for (int i = 0; i < total; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string imgPath = imagePaths[i];
                if (File.Exists(imgPath))
                {
                    var page = outputDoc.AddPage();
                    using var gfx = XGraphics.FromPdfPage(page);
                    using var img = XImage.FromFile(imgPath);

                    page.Width = img.PointWidth;
                    page.Height = img.PointHeight;
                    gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                }

                progress?.Report((double)(i + 1) / total * 100.0);
            }

            outputDoc.Save(outputPath);
            return outputPath;
        }, cancellationToken);
    }

    public async Task<string> ConvertDocumentWithLibreOfficeAsync(
        string inputPath,
        string targetExtension,
        string outputDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report(10.0);
        string result = await _libreOfficeService.ConvertFileAsync(inputPath, targetExtension, outputDirectory, cancellationToken);
        progress?.Report(100.0);
        return result;
    }
}
