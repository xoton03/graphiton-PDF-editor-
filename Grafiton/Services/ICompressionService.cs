using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface ICompressionService
{
    Task<CompressionResult> CompressPdfAsync(string inputPath, CompressionQuality quality, string outputPath);
}
