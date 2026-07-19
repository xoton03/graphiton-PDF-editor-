using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Grafiton.Services;

public class ExtractionService : IExtractionService
{
    private readonly IPdfEditService _pdfEditService;

    public ExtractionService(IPdfEditService pdfEditService)
    {
        _pdfEditService = pdfEditService;
    }

    public async Task<string> ExtractTextAsync(string pdfPath, string pageRangeExpression = "all", CancellationToken cancellationToken = default)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("Le fichier PDF est introuvable.", pdfPath);

        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            using var document = PdfDocument.Open(pdfPath);
            int totalPages = document.NumberOfPages;
            var targetPages = _pdfEditService.ParsePageRange(pageRangeExpression, totalPages);

            foreach (int pageIndex in targetPages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (pageIndex >= 1 && pageIndex <= totalPages)
                {
                    var page = document.GetPage(pageIndex);
                    sb.AppendLine($"--- Page {pageIndex} ---");
                    sb.AppendLine(page.Text);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }, cancellationToken);
    }

    public async Task ExtractTextToFileAsync(string pdfPath, string outputPath, string pageRangeExpression = "all", CancellationToken cancellationToken = default)
    {
        string text = await ExtractTextAsync(pdfPath, pageRangeExpression, cancellationToken);
        string? dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        await File.WriteAllTextAsync(outputPath, text, Encoding.UTF8, cancellationToken);
    }

    public async Task<int> ExtractImagesToFolderAsync(string pdfPath, string outputFolder, string pageRangeExpression = "all", CancellationToken cancellationToken = default)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("Le fichier PDF est introuvable.", pdfPath);

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        return await Task.Run(() =>
        {
            int imageCount = 0;
            using var document = PdfDocument.Open(pdfPath);
            int totalPages = document.NumberOfPages;
            var targetPages = _pdfEditService.ParsePageRange(pageRangeExpression, totalPages);

            foreach (int pageIndex in targetPages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (pageIndex < 1 || pageIndex > totalPages) continue;

                var page = document.GetPage(pageIndex);
                var images = page.GetImages().ToList();

                for (int i = 0; i < images.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var img = images[i];
                    imageCount++;

                    string ext = "png";

                    string fileName = $"Image_P{pageIndex}_{i + 1}.{ext}";
                    string fullPath = Path.Combine(outputFolder, fileName);

                    if (img.TryGetBytes(out var bytes))
                    {
                        File.WriteAllBytes(fullPath, bytes.ToArray());
                    }
                    else if (img.RawBytes != null && img.RawBytes.Count > 0)
                    {
                        File.WriteAllBytes(fullPath, img.RawBytes.ToArray());
                    }
                }
            }

            return imageCount;
        }, cancellationToken);
    }
}
