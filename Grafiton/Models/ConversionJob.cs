namespace Grafiton.Models;

public enum ConversionFormat
{
    PngImage,
    JpgImage,
    PdfFromImages,
    WordDocx,
    ExcelXlsx,
    PowerPointPptx,
    PdfFromOffice
}

public enum ConversionStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class ConversionJob
{
    public string SourceFilePath { get; set; } = string.Empty;
    public ConversionFormat TargetFormat { get; set; }
    public ConversionStatus Status { get; set; } = ConversionStatus.Pending;
    public double ProgressPercent { get; set; }
    public string OutputDirectory { get; set; } = string.Empty;
    public string OutputFilePath { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int Dpi { get; set; } = 150;
}
