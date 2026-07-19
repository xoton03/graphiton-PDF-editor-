using System.Collections.Generic;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IPdfEditService
{
    Task ReorderPagesAsync(string inputPath, List<int> newPageOrderIndices, string outputPath);
    Task RotatePagesAsync(string inputPath, Dictionary<int, int> pageRotations, string outputPath);
    Task DeletePagesAsync(string inputPath, List<int> pagesToDeleteIndices, string outputPath);
    Task MergePdfFilesAsync(List<string> inputFiles, string outputPath);
    Task SplitPdfFileAsync(string inputPath, string pageRangeExpression, string outputPath);
    Task AddTextWatermarkAsync(string inputPath, WatermarkOptions options, string outputPath);
    Task AddImageWatermarkAsync(string inputPath, WatermarkOptions options, string outputPath);
    Task ProtectPdfAsync(string inputPath, string userPassword, string ownerPassword, string outputPath);
    Task UnprotectPdfAsync(string inputPath, string password, string outputPath);
    List<int> ParsePageRange(string pageRangeExpression, int totalPages);
}
