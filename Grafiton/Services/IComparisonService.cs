using System.Threading;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IComparisonService
{
    Task<ComparisonResultModel> CompareDocumentsAsync(string originalPdfPath, string modifiedPdfPath, CancellationToken cancellationToken = default);
}
