using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Grafiton.Models;

public class PageComparisonResult
{
    public int PageIndex { get; set; }
    public BitmapSource? DiffImage { get; set; }
    public bool HasDifferences { get; set; }
    public int DifferencePixelCount { get; set; }
}

public class ComparisonResultModel
{
    public string OriginalPdfPath { get; set; } = string.Empty;
    public string ModifiedPdfPath { get; set; } = string.Empty;
    public List<PageComparisonResult> Pages { get; set; } = new();
    public int TotalPagesWithDiffs { get; set; }
}
