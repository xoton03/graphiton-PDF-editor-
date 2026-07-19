using System.IO;
using System.Threading.Tasks;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Grafiton.Services;

public class PdfAExportService : IPdfAExportService
{
    public Task ConvertToPdfAAsync(string inputPath, string outputPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Le fichier PDF est introuvable.", inputPath);

            using var document = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);

            // Configure PDF/A metadata options
            document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;

            document.Save(outputPath);
        });
    }
}
