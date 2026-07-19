using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Grafiton.Services;

public class LibreOfficeService : ILibreOfficeService
{
    private static readonly string[] PossibleExecutablePaths = new[]
    {
        @"C:\Program Files\LibreOffice\program\soffice.exe",
        @"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\LibreOffice\program\soffice.exe")
    };

    public bool IsLibreOfficeInstalled(out string executablePath)
    {
        foreach (string path in PossibleExecutablePaths)
        {
            if (File.Exists(path))
            {
                executablePath = path;
                return true;
            }
        }

        // Check PATH
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            foreach (string folder in pathEnv.Split(Path.PathSeparator))
            {
                string fullPath = Path.Combine(folder.Trim(), "soffice.exe");
                if (File.Exists(fullPath))
                {
                    executablePath = fullPath;
                    return true;
                }
            }
        }

        executablePath = string.Empty;
        return false;
    }

    public Task<string> ConvertFileAsync(string inputPath, string targetExtension, string outputDirectory, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            if (!IsLibreOfficeInstalled(out string sofficePath))
            {
                throw new InvalidOperationException("LibreOffice n'est pas installé sur cette machine. Veuillez installer LibreOffice pour utiliser les conversions bureautiques.");
            }

            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException("Le fichier source est introuvable.", inputPath);
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Clean extension (remove leading dot if present)
            string ext = targetExtension.TrimStart('.').ToLower();

            var startInfo = new ProcessStartInfo
            {
                FileName = sofficePath,
                Arguments = $"--headless --convert-to {ext} \"{inputPath}\" --outdir \"{outputDirectory}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var processTask = process.WaitForExitAsync(cancellationToken);
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);

            var completedTask = await Task.WhenAny(processTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                try { process.Kill(); } catch { }
                throw new TimeoutException("La conversion a dépassé le délai maximal autorisé de 2 minutes.");
            }

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
            string expectedOutputFile = Path.Combine(outputDirectory, $"{fileNameWithoutExt}.{ext}");

            if (File.Exists(expectedOutputFile))
            {
                return expectedOutputFile;
            }

            throw new InvalidOperationException($"Échec de la conversion par LibreOffice. Le fichier produit '{expectedOutputFile}' n'a pas été généré.");
        }, cancellationToken);
    }
}
