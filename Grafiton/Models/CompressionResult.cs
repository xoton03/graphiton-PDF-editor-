namespace Grafiton.Models;

public enum CompressionQuality
{
    Low,    // ~72 DPI
    Medium, // ~150 DPI
    High    // ~200 DPI
}

public class CompressionResult
{
    public string OutputFilePath { get; set; } = string.Empty;
    public long OriginalSizeBytes { get; set; }
    public long CompressedSizeBytes { get; set; }
    public double ReductionPercent => OriginalSizeBytes == 0 ? 0 : (1.0 - ((double)CompressedSizeBytes / OriginalSizeBytes)) * 100.0;

    public string OriginalSizeFormatted => FormatBytes(OriginalSizeBytes);
    public string CompressedSizeFormatted => FormatBytes(CompressedSizeBytes);
    public string ReductionFormatted => $"{ReductionPercent:F1}%";

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}
