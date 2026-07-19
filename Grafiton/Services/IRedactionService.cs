using System.Threading.Tasks;

namespace Grafiton.Services;

public interface IRedactionService
{
    Task RedactAreaAsync(string inputPath, int pageIndex, double x, double y, double width, double height, string colorHex, string outputPath);
}
