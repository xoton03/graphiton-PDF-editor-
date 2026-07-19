using System.Threading.Tasks;

namespace Grafiton.Services;

public interface IPdfAExportService
{
    Task ConvertToPdfAAsync(string inputPath, string outputPath);
}
