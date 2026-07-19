using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace Grafiton.Services;

public class OcrService : IOcrService
{
    public async Task<string> RecognizeTextAsync(string pdfPath, int pageIndex, string language = "fr-FR", CancellationToken cancellationToken = default)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("Le fichier PDF est introuvable.", pdfPath);

        using var fileStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var winrtStream = fileStream.AsRandomAccessStream();
        var pdfDoc = await PdfDocument.LoadFromStreamAsync(winrtStream);

        if (pageIndex < 0 || pageIndex >= (int)pdfDoc.PageCount)
            return string.Empty;

        using var pdfPage = pdfDoc.GetPage((uint)pageIndex);
        using var memStream = new InMemoryRandomAccessStream();

        var renderOptions = new PdfPageRenderOptions
        {
            DestinationWidth = (uint)(pdfPage.Size.Width * 2) // 2x scale for higher OCR precision
        };

        await pdfPage.RenderToStreamAsync(memStream, renderOptions);

        var decoder = await BitmapDecoder.CreateAsync(memStream);
        using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

        var lang = new Windows.Globalization.Language(language);
        var ocrEngine = OcrEngine.TryCreateFromLanguage(lang) ?? OcrEngine.TryCreateFromUserProfileLanguages();

        if (ocrEngine == null)
            throw new InvalidOperationException("Le moteur OCR Windows n'est pas disponible pour cette langue.");

        var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

        var sb = new StringBuilder();
        foreach (var line in ocrResult.Lines)
        {
            sb.AppendLine(line.Text);
        }

        return sb.ToString();
    }

    public async Task<string> RecognizeDocumentTextAsync(string pdfPath, string language = "fr-FR", CancellationToken cancellationToken = default)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("Le fichier PDF est introuvable.", pdfPath);

        using var fileStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var winrtStream = fileStream.AsRandomAccessStream();
        var pdfDoc = await PdfDocument.LoadFromStreamAsync(winrtStream);

        var sb = new StringBuilder();
        for (int i = 0; i < (int)pdfDoc.PageCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string pageText = await RecognizeTextAsync(pdfPath, i, language, cancellationToken);
            sb.AppendLine($"--- Page {i + 1} ---");
            sb.AppendLine(pageText);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
