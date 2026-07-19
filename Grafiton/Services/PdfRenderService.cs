using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using UglyToad.PdfPig;

namespace Grafiton.Services;

public class PdfRenderService : IPdfRenderService
{
    public Task<int> GetPageCountAsync(string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                if (!File.Exists(filePath)) return 0;
                using var document = UglyToad.PdfPig.PdfDocument.Open(filePath);
                return document.NumberOfPages;
            }
            catch
            {
                return 0;
            }
        });
    }

    public async Task<BitmapSource?> RenderPageThumbnailAsync(string filePath, uint pageIndex, uint targetWidth = 180)
    {
        try
        {
            if (!File.Exists(filePath)) return null;

            var storageFile = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(filePath));
            var pdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(storageFile);

            if (pageIndex >= pdfDoc.PageCount) return null;

            using var page = pdfDoc.GetPage(pageIndex);
            using var stream = new InMemoryRandomAccessStream();

            var options = new PdfPageRenderOptions
            {
                DestinationWidth = targetWidth
            };

            await page.RenderToStreamAsync(stream, options);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream.AsStreamForRead();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<BitmapSource?>> RenderAllThumbnailsAsync(string filePath, uint targetWidth = 180)
    {
        var result = new List<BitmapSource?>();
        int pageCount = await GetPageCountAsync(filePath);

        for (uint i = 0; i < pageCount; i++)
        {
            var thumbnail = await RenderPageThumbnailAsync(filePath, i, targetWidth);
            result.Add(thumbnail);
        }

        return result;
    }
}
