using System.Collections.Generic;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IAnnotationService
{
    Task AddAnnotationAsync(string pdfPath, AnnotationModel annotation, string outputPath);
    Task AddAnnotationsAsync(string pdfPath, List<AnnotationModel> annotations, string outputPath);
}
