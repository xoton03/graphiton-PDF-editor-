using System;
using System.IO;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Grafiton.Services;

public class RedactionService : IRedactionService
{
    public Task RedactAreaAsync(string inputPath, int pageIndex, double x, double y, double width, double height, string colorHex, string outputPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Le fichier PDF est introuvable.", inputPath);

            using var document = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);

            if (pageIndex < 0 || pageIndex >= document.PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "Index de page invalide.");

            var page = document.Pages[pageIndex];

            // Parse redaction mask color from Hex or fallback to Black
            XColor maskColor = XColors.Black;
            try
            {
                if (!string.IsNullOrWhiteSpace(colorHex))
                {
                    var sysColor = System.Drawing.ColorTranslator.FromHtml(colorHex);
                    maskColor = XColor.FromArgb(sysColor.A, sysColor.R, sysColor.G, sysColor.B);
                }
            }
            catch
            {
                maskColor = XColors.Black;
            }

            using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
            var brush = new XSolidBrush(maskColor);
            gfx.DrawRectangle(brush, x, y, width, height);

            document.Save(outputPath);
        });
    }
}
