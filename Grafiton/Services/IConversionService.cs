using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IConversionService
{
    Task<List<string>> ExportPdfToImagesAsync(string pdfPath, string format, int dpi, string outputDirectory, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    Task<string> ConvertImagesToPdfAsync(List<string> imagePaths, string outputPath, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    Task<string> ConvertDocumentWithLibreOfficeAsync(string inputPath, string targetExtension, string outputDirectory, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
