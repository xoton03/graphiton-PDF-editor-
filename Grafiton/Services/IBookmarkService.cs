using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IBookmarkService
{
    Task<ObservableCollection<BookmarkModel>> GetBookmarksAsync(string pdfPath);
}
