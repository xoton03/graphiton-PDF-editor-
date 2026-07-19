using System;
using System.IO;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using Grafiton.Models;

namespace Grafiton.Services;

public class HeaderFooterService : IHeaderFooterService
{
    private readonly IPdfEditService _pdfEditService;

    public HeaderFooterService(IPdfEditService pdfEditService)
    {
        _pdfEditService = pdfEditService;
    }

    public Task AddHeaderFooterAsync(string inputPath, HeaderFooterOptions options, string outputPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Le fichier PDF est introuvable.", inputPath);

            using var document = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);
            int totalPages = document.PageCount;
            var targetPages = _pdfEditService.ParsePageRange(options.PageRange, totalPages);

            var font = new XFont(options.FontFamily ?? "Arial", options.FontSize, XFontStyle.Regular);
            var brush = XBrushes.DarkGray;

            foreach (int pageIndex in targetPages)
            {
                if (pageIndex < 1 || pageIndex > totalPages) continue;

                var page = document.Pages[pageIndex - 1];
                using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

                double margin = 25;
                double pageWidth = page.Width;
                double pageHeight = page.Height;

                // Draw Header Text
                if (!string.IsNullOrWhiteSpace(options.HeaderText))
                {
                    gfx.DrawString(options.HeaderText, font, brush,
                        new XRect(margin, margin, pageWidth - (2 * margin), 20),
                        XStringFormats.TopCenter);
                }

                // Draw Footer Text
                if (!string.IsNullOrWhiteSpace(options.FooterText))
                {
                    gfx.DrawString(options.FooterText, font, brush,
                        new XRect(margin, pageHeight - margin - 20, pageWidth - (2 * margin), 20),
                        XStringFormats.BottomCenter);
                }

                // Draw Page Numbers
                if (options.IncludePageNumbers)
                {
                    string numText = string.Format(options.PageNumberFormat, pageIndex, totalPages);
                    var rect = GetNumberPositionRect(options.NumberPosition, pageWidth, pageHeight, margin);
                    var format = GetNumberStringFormat(options.NumberPosition);

                    gfx.DrawString(numText, font, brush, rect, format);
                }
            }

            document.Save(outputPath);
        });
    }

    private static XRect GetNumberPositionRect(PageNumberPosition pos, double width, double height, double margin)
    {
        return pos switch
        {
            PageNumberPosition.TopLeft => new XRect(margin, margin, width - (2 * margin), 20),
            PageNumberPosition.TopCenter => new XRect(margin, margin, width - (2 * margin), 20),
            PageNumberPosition.TopRight => new XRect(margin, margin, width - (2 * margin), 20),
            PageNumberPosition.BottomLeft => new XRect(margin, height - margin - 20, width - (2 * margin), 20),
            PageNumberPosition.BottomRight => new XRect(margin, height - margin - 20, width - (2 * margin), 20),
            _ => new XRect(margin, height - margin - 20, width - (2 * margin), 20)
        };
    }

    private static XStringFormat GetNumberStringFormat(PageNumberPosition pos)
    {
        return pos switch
        {
            PageNumberPosition.TopLeft => XStringFormats.TopLeft,
            PageNumberPosition.TopCenter => XStringFormats.TopCenter,
            PageNumberPosition.TopRight => XStringFormats.TopRight,
            PageNumberPosition.BottomLeft => XStringFormats.BottomLeft,
            PageNumberPosition.BottomRight => XStringFormats.BottomRight,
            _ => XStringFormats.BottomCenter
        };
    }
}
