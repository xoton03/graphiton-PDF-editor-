using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Grafiton.Services;

public interface IPdfRenderService
{
    Task<int> GetPageCountAsync(string filePath);
    Task<BitmapSource?> RenderPageThumbnailAsync(string filePath, uint pageIndex, uint targetWidth = 180);
    Task<List<BitmapSource?>> RenderAllThumbnailsAsync(string filePath, uint targetWidth = 180);
}
