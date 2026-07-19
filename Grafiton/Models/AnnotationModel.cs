using System.Collections.Generic;
using System.Windows;

namespace Grafiton.Models;

public enum AnnotationType
{
    Highlight,
    StickyNote,
    InkDrawing,
    ShapeRectangle,
    ShapeEllipse,
    ShapeLine,
    ShapeArrow
}

public class AnnotationModel
{
    public string Id { get; set; } = System.Guid.NewGuid().ToString();
    public AnnotationType Type { get; set; }
    public int PageIndex { get; set; }
    public Rect BoundingBox { get; set; }
    public string HexColor { get; set; } = "#FFD700"; // Gold/Yellow default
    public double StrokeThickness { get; set; } = 2.0;
    public double Opacity { get; set; } = 0.8;
    public string NoteContent { get; set; } = string.Empty;
    public List<Point> InkPoints { get; set; } = new();
}
