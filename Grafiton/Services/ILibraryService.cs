using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface ILibraryService
{
    Task<ObservableCollection<LibraryEntry>> GetLibraryEntriesAsync();
    Task AddOrUpdateEntryAsync(string filePath, bool isFavorite = false);
    Task ToggleFavoriteAsync(string filePath);
    Task RemoveEntryAsync(string filePath);
}
