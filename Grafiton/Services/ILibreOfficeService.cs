using System.Threading;
using System.Threading.Tasks;

namespace Grafiton.Services;

public interface ILibreOfficeService
{
    bool IsLibreOfficeInstalled(out string executablePath);
    Task<string> ConvertFileAsync(string inputPath, string targetExtension, string outputDirectory, CancellationToken cancellationToken = default);
}
