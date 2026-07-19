namespace Grafiton.Models;

public enum BatchOperationType
{
    Watermark,
    Compress,
    ConvertToImages,
    ConvertToDocx,
    ProtectPassword,
    Rotate
}

public enum BatchJobStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class BatchJob
{
    public string SourceFilePath { get; set; } = string.Empty;
    public string FileName => System.IO.Path.GetFileName(SourceFilePath);
    public BatchOperationType Operation { get; set; }
    public BatchJobStatus Status { get; set; } = BatchJobStatus.Pending;
    public double ProgressPercent { get; set; }
    public string OutputDirectory { get; set; } = string.Empty;
    public string OutputFilePath { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string WatermarkText { get; set; } = "CONFIDENTIAL";
    public string Password { get; set; } = string.Empty;
}
