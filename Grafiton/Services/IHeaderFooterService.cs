using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IHeaderFooterService
{
    Task AddHeaderFooterAsync(string inputPath, HeaderFooterOptions options, string outputPath);
}
