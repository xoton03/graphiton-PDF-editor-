using System;
using System.IO;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Grafiton.Services;

public class SignatureService : ISignatureService
{
    public Task StampSignatureAsync(string inputPdfPath, string signatureImagePath, int pageIndex, double x, double y, double width, double height, string outputPdfPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(inputPdfPath))
                throw new FileNotFoundException("Document PDF introuvable.", inputPdfPath);

            if (!File.Exists(signatureImagePath))
                throw new FileNotFoundException("Image de signature introuvable.", signatureImagePath);

            using var doc = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Modify);

            if (pageIndex < 0 || pageIndex >= doc.PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "Numéro de page invalide.");

            var page = doc.Pages[pageIndex];
            using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
            using var image = XImage.FromFile(signatureImagePath);

            gfx.DrawImage(image, x, y, width, height);

            doc.Save(outputPdfPath);
        });
    }
}
