namespace Grafiton.Models;

public enum WatermarkType
{
    Text,
    Image
}

public enum WatermarkPosition
{
    Center,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Tile
}

public class WatermarkOptions
{
    public WatermarkType Type { get; set; } = WatermarkType.Text;
    public string Text { get; set; } = "CONFIDENTIEL";
    public string FontFamily { get; set; } = "Arial";
    public double FontSize { get; set; } = 48.0;
    public string ImagePath { get; set; } = string.Empty;
    public double Opacity { get; set; } = 0.3; // 0.0 - 1.0
    public double RotationAngle { get; set; } = -45.0; // Angle en degrés
    public WatermarkPosition Position { get; set; } = WatermarkPosition.Center;
    public string HexColor { get; set; } = "#808080";
    public string PageRange { get; set; } = "all"; // "all" ou ex: "1-5"
}
