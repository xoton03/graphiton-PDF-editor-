using System.Threading.Tasks;

namespace Grafiton.Services;

public interface ISignatureService
{
    Task StampSignatureAsync(string inputPdfPath, string signatureImagePath, int pageIndex, double x, double y, double width, double height, string outputPdfPath);
}
