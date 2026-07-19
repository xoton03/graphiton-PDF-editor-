namespace Grafiton.Models;

public enum PageNumberPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public class HeaderFooterOptions
{
    public string HeaderText { get; set; } = string.Empty;
    public string FooterText { get; set; } = string.Empty;
    public bool IncludePageNumbers { get; set; } = true;
    public PageNumberPosition NumberPosition { get; set; } = PageNumberPosition.BottomCenter;
    public string PageNumberFormat { get; set; } = "Page {0} sur {1}";
    public double FontSize { get; set; } = 10.0;
    public string FontFamily { get; set; } = "Arial";
    public string PageRange { get; set; } = "all";
}
