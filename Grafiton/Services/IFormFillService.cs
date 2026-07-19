using System.Collections.Generic;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IFormFillService
{
    Task<List<FormItemModel>> GetFormFieldsAsync(string pdfPath);
    Task FillFormFieldsAsync(string pdfPath, List<FormItemModel> fields, bool flatten, string outputPath);
}
