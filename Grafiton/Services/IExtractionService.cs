using System.Threading;
using System.Threading.Tasks;

namespace Grafiton.Services;

public interface IExtractionService
{
    Task<string> ExtractTextAsync(string pdfPath, string pageRangeExpression = "all", CancellationToken cancellationToken = default);
    Task ExtractTextToFileAsync(string pdfPath, string outputPath, string pageRangeExpression = "all", CancellationToken cancellationToken = default);
    Task<int> ExtractImagesToFolderAsync(string pdfPath, string outputFolder, string pageRangeExpression = "all", CancellationToken cancellationToken = default);
}
