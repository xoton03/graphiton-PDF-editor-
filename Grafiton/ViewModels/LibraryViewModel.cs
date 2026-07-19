using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
    private readonly ILibraryService _libraryService;
    private readonly Action<string>? _openPdfCallback;

    [ObservableProperty]
    private ObservableCollection<LibraryEntry> _entries = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _showFavoritesOnly;

    public LibraryViewModel(ILibraryService libraryService, Action<string>? openPdfCallback = null)
    {
        _libraryService = libraryService;
        _openPdfCallback = openPdfCallback;
    }

    public async Task LoadLibraryAsync()
    {
        var all = await _libraryService.GetLibraryEntriesAsync();
        if (ShowFavoritesOnly)
        {
            Entries = new ObservableCollection<LibraryEntry>(all.Where(e => e.IsFavorite));
        }
        else if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            Entries = new ObservableCollection<LibraryEntry>(all.Where(e => e.FileName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));
        }
        else
        {
            Entries = all;
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(LibraryEntry entry)
    {
        if (entry != null)
        {
            await _libraryService.ToggleFavoriteAsync(entry.FilePath);
            await LoadLibraryAsync();
        }
    }

    [RelayCommand]
    private void OpenEntry(LibraryEntry entry)
    {
        if (entry != null && System.IO.File.Exists(entry.FilePath))
        {
            _openPdfCallback?.Invoke(entry.FilePath);
        }
    }
}
