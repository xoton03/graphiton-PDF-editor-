using System;

namespace Grafiton.Models;

public class LibraryEntry
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public DateTime AddedDate { get; set; } = DateTime.Now;
    public DateTime LastOpened { get; set; } = DateTime.Now;
    public long FileSizeBytes { get; set; }
    public int PageCount { get; set; }
    public string FormattedSize => FileSizeBytes < 1024 * 1024 ? $"{FileSizeBytes / 1024.0:F1} KB" : $"{FileSizeBytes / (1024.0 * 1024.0):F1} MB";
}
