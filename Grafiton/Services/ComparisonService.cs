using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Grafiton.Models;

namespace Grafiton.Services;

public class ComparisonService : IComparisonService
{
    public async Task<ComparisonResultModel> CompareDocumentsAsync(string originalPdfPath, string modifiedPdfPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(originalPdfPath))
            throw new FileNotFoundException("Le premier document PDF est introuvable.", originalPdfPath);
        if (!File.Exists(modifiedPdfPath))
            throw new FileNotFoundException("Le second document PDF est introuvable.", modifiedPdfPath);

        var result = new ComparisonResultModel
        {
            OriginalPdfPath = originalPdfPath,
            ModifiedPdfPath = modifiedPdfPath
        };

        using var stream1 = new FileStream(originalPdfPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var winStream1 = stream1.AsRandomAccessStream();
        var pdf1 = await PdfDocument.LoadFromStreamAsync(winStream1);

        using var stream2 = new FileStream(modifiedPdfPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var winStream2 = stream2.AsRandomAccessStream();
        var pdf2 = await PdfDocument.LoadFromStreamAsync(winStream2);

        int maxPages = Math.Max((int)pdf1.PageCount, (int)pdf2.PageCount);
        int diffPagesCount = 0;

        for (int i = 0; i < maxPages; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[]? bgra1 = i < (int)pdf1.PageCount ? await RenderPageToBgraAsync(pdf1.GetPage((uint)i)) : null;
            byte[]? bgra2 = i < (int)pdf2.PageCount ? await RenderPageToBgraAsync(pdf2.GetPage((uint)i)) : null;

            var (diffBmp, pixelDiffs) = CreateOverlayDiffBitmap(bgra1, bgra2, 800, 1100);

            bool hasDiff = pixelDiffs > 50;
            if (hasDiff) diffPagesCount++;

            result.Pages.Add(new PageComparisonResult
            {
                PageIndex = i,
                DiffImage = diffBmp,
                HasDifferences = hasDiff,
                DifferencePixelCount = pixelDiffs
            });
        }

        result.TotalPagesWithDiffs = diffPagesCount;
        return result;
    }

    private static async Task<byte[]> RenderPageToBgraAsync(PdfPage page)
    {
        using (page)
        {
            using var stream = new InMemoryRandomAccessStream();
            var opts = new PdfPageRenderOptions { DestinationWidth = 800 };
            await page.RenderToStreamAsync(stream, opts);

            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            return pixelData.DetachPixelData();
        }
    }

    private static (BitmapSource diffBitmap, int pixelDiffCount) CreateOverlayDiffBitmap(byte[]? bgra1, byte[]? bgra2, int width, int height)
    {
        int bytesPerPixel = 4;
        int totalPixels = width * height;
        byte[] outputPixels = new byte[totalPixels * bytesPerPixel];
        int diffCount = 0;

        for (int i = 0; i < outputPixels.Length; i += 4)
        {
            byte r1 = bgra1 != null && i + 2 < bgra1.Length ? bgra1[i + 2] : (byte)255;
            byte g1 = bgra1 != null && i + 1 < bgra1.Length ? bgra1[i + 1] : (byte)255;
            byte b1 = bgra1 != null && i < bgra1.Length ? bgra1[i] : (byte)255;

            byte r2 = bgra2 != null && i + 2 < bgra2.Length ? bgra2[i + 2] : (byte)255;
            byte g2 = bgra2 != null && i + 1 < bgra2.Length ? bgra2[i + 1] : (byte)255;
            byte b2 = bgra2 != null && i < bgra2.Length ? bgra2[i] : (byte)255;

            int deltaR = Math.Abs(r1 - r2);
            int deltaG = Math.Abs(g1 - g2);
            int deltaB = Math.Abs(b1 - b2);

            if (deltaR + deltaG + deltaB > 40)
            {
                // Highlight difference in vibrant Magenta/Red (#FF0055)
                outputPixels[i] = 0x55;     // Blue
                outputPixels[i + 1] = 0x00; // Green
                outputPixels[i + 2] = 0xFF; // Red
                outputPixels[i + 3] = 0xFF; // Alpha
                diffCount++;
            }
            else
            {
                // Render identical content in subtle dimmed grayscale
                byte gray = (byte)((r1 * 0.3) + (g1 * 0.59) + (b1 * 0.11));
                outputPixels[i] = gray;
                outputPixels[i + 1] = gray;
                outputPixels[i + 2] = gray;
                outputPixels[i + 3] = 0xFF;
            }
        }

        var writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        writeableBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), outputPixels, width * bytesPerPixel, 0);
        writeableBitmap.Freeze();

        return (writeableBitmap, diffCount);
    }
}
