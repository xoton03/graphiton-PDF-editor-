using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using Grafiton.Models;

namespace Grafiton.Services;

public class AnnotationService : IAnnotationService
{
    private static XColor ParseHexColor(string hexColor)
    {
        try
        {
            var sysColor = System.Drawing.ColorTranslator.FromHtml(hexColor);
            return XColor.FromArgb(sysColor.A, sysColor.R, sysColor.G, sysColor.B);
        }
        catch
        {
            return XColors.Gold;
        }
    }

    public Task AddAnnotationAsync(string pdfPath, AnnotationModel annotation, string outputPath)
    {
        return AddAnnotationsAsync(pdfPath, new List<AnnotationModel> { annotation }, outputPath);
    }

    public Task AddAnnotationsAsync(string pdfPath, List<AnnotationModel> annotations, string outputPath)
    {
        return Task.Run(() =>
        {
            if (annotations == null || annotations.Count == 0) return;

            using var doc = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Modify);

            foreach (var ann in annotations)
            {
                if (ann.PageIndex >= 0 && ann.PageIndex < doc.PageCount)
                {
                    var page = doc.Pages[ann.PageIndex];
                    using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

                    XColor color = ParseHexColor(ann.HexColor);

                    switch (ann.Type)
                    {
                        case AnnotationType.Highlight:
                            var highlightBrush = new XSolidBrush(XColor.FromArgb((byte)(ann.Opacity * 255), color));
                            gfx.DrawRectangle(highlightBrush, ann.BoundingBox.X, ann.BoundingBox.Y, ann.BoundingBox.Width, ann.BoundingBox.Height);
                            break;

                        case AnnotationType.StickyNote:
                            var noteBrush = new XSolidBrush(XColor.FromArgb(220, color));
                            var noteBorderPen = new XPen(XColors.DarkGray, 1);
                            gfx.DrawRectangle(noteBorderPen, noteBrush, ann.BoundingBox.X, ann.BoundingBox.Y, 24, 24);
                            if (!string.IsNullOrEmpty(ann.NoteContent))
                            {
                                var font = new XFont("Arial", 8);
                                gfx.DrawString("N", font, XBrushes.Black, new XPoint(ann.BoundingBox.X + 6, ann.BoundingBox.Y + 16));
                            }
                            break;

                        case AnnotationType.InkDrawing:
                            if (ann.InkPoints.Count > 1)
                            {
                                var pen = new XPen(color, ann.StrokeThickness);
                                for (int i = 0; i < ann.InkPoints.Count - 1; i++)
                                {
                                    var p1 = new XPoint(ann.InkPoints[i].X, ann.InkPoints[i].Y);
                                    var p2 = new XPoint(ann.InkPoints[i + 1].X, ann.InkPoints[i + 1].Y);
                                    gfx.DrawLine(pen, p1, p2);
                                }
                            }
                            break;

                        case AnnotationType.ShapeRectangle:
                            var rectPen = new XPen(color, ann.StrokeThickness);
                            gfx.DrawRectangle(rectPen, ann.BoundingBox.X, ann.BoundingBox.Y, ann.BoundingBox.Width, ann.BoundingBox.Height);
                            break;

                        case AnnotationType.ShapeEllipse:
                            var ellipsePen = new XPen(color, ann.StrokeThickness);
                            gfx.DrawEllipse(ellipsePen, ann.BoundingBox.X, ann.BoundingBox.Y, ann.BoundingBox.Width, ann.BoundingBox.Height);
                            break;

                        case AnnotationType.ShapeLine:
                        case AnnotationType.ShapeArrow:
                            var linePen = new XPen(color, ann.StrokeThickness);
                            gfx.DrawLine(linePen,
                                new XPoint(ann.BoundingBox.Left, ann.BoundingBox.Top),
                                new XPoint(ann.BoundingBox.Right, ann.BoundingBox.Bottom));
                            break;
                    }
                }
            }

            doc.Save(outputPath);
        });
    }
}
