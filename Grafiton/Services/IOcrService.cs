using System.Threading;
using System.Threading.Tasks;

namespace Grafiton.Services;

public interface IOcrService
{
    Task<string> RecognizeTextAsync(string pdfPath, int pageIndex, string language = "fr-FR", CancellationToken cancellationToken = default);
    Task<string> RecognizeDocumentTextAsync(string pdfPath, string language = "fr-FR", CancellationToken cancellationToken = default);
}
