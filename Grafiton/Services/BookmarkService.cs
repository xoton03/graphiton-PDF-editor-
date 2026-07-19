using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Grafiton.Models;
using PdfSharpCore.Pdf.IO;

namespace Grafiton.Services;

public class BookmarkService : IBookmarkService
{
    public Task<ObservableCollection<BookmarkModel>> GetBookmarksAsync(string pdfPath)
    {
        return Task.Run(() =>
        {
            var result = new ObservableCollection<BookmarkModel>();
            if (!File.Exists(pdfPath)) return result;

            try
            {
                using var doc = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
                if (doc.Outlines != null && doc.Outlines.Count > 0)
                {
                    int pageCounter = 1;
                    foreach (var outline in doc.Outlines)
                    {
                        var bm = new BookmarkModel
                        {
                            Title = outline.Title,
                            PageNumber = pageCounter++
                        };
                        result.Add(bm);
                    }
                }
            }
            catch
            {
                // Fallback: Return empty list if PDF outlines are corrupt or missing
            }

            return result;
        });
    }
}
