using System.Collections.ObjectModel;

namespace Grafiton.Models;

public class BookmarkModel
{
    public string Title { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public ObservableCollection<BookmarkModel> Children { get; set; } = new();
}
