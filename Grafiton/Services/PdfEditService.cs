using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using Grafiton.Models;

namespace Grafiton.Services;

public class PdfEditService : IPdfEditService
{
    private static XColor ParseHexColor(string hexColor)
    {
        try
        {
            var sysColor = System.Drawing.ColorTranslator.FromHtml(hexColor);
            return XColor.FromArgb(sysColor.A, sysColor.R, sysColor.G, sysColor.B);
        }
        catch
        {
            return XColors.Gray;
        }
    }

    public Task ReorderPagesAsync(string inputPath, List<int> newPageOrderIndices, string outputPath)
    {
        return Task.Run(() =>
        {
            using var inputDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
            using var outputDoc = new PdfDocument();

            foreach (int index in newPageOrderIndices)
            {
                if (index >= 0 && index < inputDoc.PageCount)
                {
                    outputDoc.AddPage(inputDoc.Pages[index]);
                }
            }

            outputDoc.Save(outputPath);
        });
    }

    public Task RotatePagesAsync(string inputPath, Dictionary<int, int> pageRotations, string outputPath)
    {
        return Task.Run(() =>
        {
            using var doc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);

            foreach (var kvp in pageRotations)
            {
                int pageIndex = kvp.Key;
                int angleDelta = kvp.Value;

                if (pageIndex >= 0 && pageIndex < doc.PageCount)
                {
                    var page = doc.Pages[pageIndex];
                    int currentRotate = page.Orientation == PdfSharpCore.PageOrientation.Landscape ? 90 : 0;
                    if (page.Elements.ContainsKey("/Rotate"))
                    {
                        currentRotate = page.Elements.GetInteger("/Rotate");
                    }
                    int newAngle = (currentRotate + angleDelta) % 360;
                    page.Rotate = newAngle;
                }
            }

            doc.Save(outputPath);
        });
    }

    public Task DeletePagesAsync(string inputPath, List<int> pagesToDeleteIndices, string outputPath)
    {
        return Task.Run(() =>
        {
            using var inputDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
            using var outputDoc = new PdfDocument();
            var deleteSet = new HashSet<int>(pagesToDeleteIndices);

            for (int i = 0; i < inputDoc.PageCount; i++)
            {
                if (!deleteSet.Contains(i))
                {
                    outputDoc.AddPage(inputDoc.Pages[i]);
                }
            }

            outputDoc.Save(outputPath);
        });
    }

    public Task MergePdfFilesAsync(List<string> inputFiles, string outputPath)
    {
        return Task.Run(() =>
        {
            using var outputDoc = new PdfDocument();

            foreach (string file in inputFiles)
            {
                if (File.Exists(file))
                {
                    using var inputDoc = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                    for (int i = 0; i < inputDoc.PageCount; i++)
                    {
                        outputDoc.AddPage(inputDoc.Pages[i]);
                    }
                }
            }

            outputDoc.Save(outputPath);
        });
    }

    public Task SplitPdfFileAsync(string inputPath, string pageRangeExpression, string outputPath)
    {
        return Task.Run(() =>
        {
            using var inputDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
            using var outputDoc = new PdfDocument();

            var selectedPages = ParsePageRange(pageRangeExpression, inputDoc.PageCount);

            foreach (int pageIndex in selectedPages)
            {
                if (pageIndex >= 0 && pageIndex < inputDoc.PageCount)
                {
                    outputDoc.AddPage(inputDoc.Pages[pageIndex]);
                }
            }

            outputDoc.Save(outputPath);
        });
    }

    public Task AddTextWatermarkAsync(string inputPath, WatermarkOptions options, string outputPath)
    {
        return Task.Run(() =>
        {
            using var doc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);
            var selectedPages = ParsePageRange(options.PageRange, doc.PageCount);
            var pageSet = new HashSet<int>(selectedPages);
            var baseColor = ParseHexColor(options.HexColor);

            for (int i = 0; i < doc.PageCount; i++)
            {
                if (pageSet.Contains(i))
                {
                    var page = doc.Pages[i];
                    using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

                    var font = new XFont(options.FontFamily, options.FontSize, XFontStyle.Bold);
                    var brush = new XSolidBrush(XColor.FromArgb(
                        (byte)(options.Opacity * 255),
                        baseColor
                    ));

                    var size = gfx.MeasureString(options.Text, font);

                    gfx.TranslateTransform(page.Width / 2, page.Height / 2);
                    gfx.RotateTransform(options.RotationAngle);
                    gfx.DrawString(options.Text, font, brush, new XPoint(-size.Width / 2, size.Height / 4));
                }
            }

            doc.Save(outputPath);
        });
    }

    public Task AddImageWatermarkAsync(string inputPath, WatermarkOptions options, string outputPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(options.ImagePath)) return;

            using var doc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);
            var selectedPages = ParsePageRange(options.PageRange, doc.PageCount);
            var pageSet = new HashSet<int>(selectedPages);
            using var image = XImage.FromFile(options.ImagePath);

            for (int i = 0; i < doc.PageCount; i++)
            {
                if (pageSet.Contains(i))
                {
                    var page = doc.Pages[i];
                    using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

                    double width = image.PixelWidth * 0.5;
                    double height = image.PixelHeight * 0.5;

                    gfx.TranslateTransform(page.Width / 2, page.Height / 2);
                    gfx.RotateTransform(options.RotationAngle);
                    gfx.DrawImage(image, -width / 2, -height / 2, width, height);
                }
            }

            doc.Save(outputPath);
        });
    }

    public Task ProtectPdfAsync(string inputPath, string userPassword, string ownerPassword, string outputPath)
    {
        return Task.Run(() =>
        {
            using var doc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);
            var security = doc.SecuritySettings;
            security.UserPassword = userPassword;
            security.OwnerPassword = string.IsNullOrEmpty(ownerPassword) ? userPassword : ownerPassword;
            doc.Save(outputPath);
        });
    }

    public Task UnprotectPdfAsync(string inputPath, string password, string outputPath)
    {
        return Task.Run(() =>
        {
            using var doc = PdfReader.Open(inputPath, password, PdfDocumentOpenMode.Modify);
            doc.Save(outputPath);
        });
    }

    public List<int> ParsePageRange(string pageRangeExpression, int totalPages)
    {
        var result = new List<int>();
        if (string.IsNullOrWhiteSpace(pageRangeExpression) || pageRangeExpression.Trim().Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return Enumerable.Range(0, totalPages).ToList();
        }

        string[] parts = pageRangeExpression.Split(',');
        foreach (string part in parts)
        {
            string p = part.Trim();
            if (p.Contains("-"))
            {
                string[] range = p.Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                {
                    start = Math.Max(1, start);
                    end = Math.Min(totalPages, end);
                    for (int i = start; i <= end; i++)
                    {
                        result.Add(i - 1);
                    }
                }
            }
            else if (int.TryParse(p, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages)
                {
                    result.Add(pageNum - 1);
                }
            }
        }

        return result.Distinct().OrderBy(x => x).ToList();
    }
}
