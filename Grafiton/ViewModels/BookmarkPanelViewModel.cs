using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class BookmarkPanelViewModel : ObservableObject
{
    private readonly IBookmarkService _bookmarkService;
    private readonly Action<int>? _navigateToPageCallback;

    [ObservableProperty]
    private ObservableCollection<BookmarkModel> _bookmarks = new();

    [ObservableProperty]
    private BookmarkModel? _selectedBookmark;

    public BookmarkPanelViewModel(IBookmarkService bookmarkService, Action<int>? navigateToPageCallback = null)
    {
        _bookmarkService = bookmarkService;
        _navigateToPageCallback = navigateToPageCallback;
    }

    public async Task LoadBookmarksAsync(string pdfPath)
    {
        Bookmarks = await _bookmarkService.GetBookmarksAsync(pdfPath);
    }

    [RelayCommand]
    private void NavigateToBookmark(BookmarkModel? bookmark)
    {
        if (bookmark != null)
        {
            _navigateToPageCallback?.Invoke(bookmark.PageNumber);
        }
    }
}
