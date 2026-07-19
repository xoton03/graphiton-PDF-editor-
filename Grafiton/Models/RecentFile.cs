using System;

namespace Grafiton.Models;

public class RecentFile
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime LastOpened { get; set; } = DateTime.Now;
}
