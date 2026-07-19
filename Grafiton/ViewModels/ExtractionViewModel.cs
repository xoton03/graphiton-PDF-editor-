using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public enum ExtractionMode
{
    Text,
    Images
}

public partial class ExtractionViewModel : ObservableObject
{
    private readonly IExtractionService _extractionService;
    private readonly IFileService _fileService;

    [ObservableProperty]
    private string _currentPdfPath = string.Empty;

    [ObservableProperty]
    private bool _isTextMode = true;

    [ObservableProperty]
    private bool _isImagesMode = false;

    public ExtractionMode Mode => IsTextMode ? ExtractionMode.Text : ExtractionMode.Images;

    [ObservableProperty]
    private string _pageRange = "all";

    [ObservableProperty]
    private string _outputDirectory = string.Empty;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = "Sélectionnez le mode d'extraction et le dossier de destination.";

    public event Action? RequestClose;

    public ExtractionViewModel(IExtractionService extractionService, IFileService fileService)
    {
        _extractionService = extractionService;
        _fileService = fileService;
    }

    [RelayCommand]
    private void BrowseOutputDirectory()
    {
        if (IsTextMode)
        {
            string baseName = string.IsNullOrEmpty(CurrentPdfPath) ? "TexteExtrait" : $"{Path.GetFileNameWithoutExtension(CurrentPdfPath)}_TexteExtrait";
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Enregistrer le fichier texte extrait",
                Filter = "Fichier texte (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = $"{baseName}.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                OutputDirectory = saveDialog.FileName;
            }
        }
        else
        {
            var folderDialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Sélectionner le dossier de destination pour les images"
            };

            if (folderDialog.ShowDialog() == true)
            {
                OutputDirectory = folderDialog.FolderName;
            }
        }
    }

    [RelayCommand]
    private async Task StartExtractionAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPdfPath) || !File.Exists(CurrentPdfPath))
        {
            StatusMessage = "Aucun document PDF ouvert.";
            return;
        }

        IsProcessing = true;

        try
        {
            string baseName = Path.GetFileNameWithoutExtension(CurrentPdfPath);

            if (Mode == ExtractionMode.Text)
            {
                string txtPath = OutputDirectory;
                if (string.IsNullOrWhiteSpace(txtPath) || Directory.Exists(txtPath))
                {
                    string dir = string.IsNullOrWhiteSpace(txtPath)
                        ? Path.GetDirectoryName(CurrentPdfPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                        : txtPath;
                    txtPath = Path.Combine(dir, $"{baseName}_TexteExtrait.txt");
                }

                if (!txtPath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    txtPath += ".txt";
                }

                StatusMessage = "Extraction du texte en cours...";
                await _extractionService.ExtractTextToFileAsync(CurrentPdfPath, txtPath, PageRange);
                StatusMessage = $"Texte extrait avec succès dans : {Path.GetFileName(txtPath)}";
            }
            else
            {
                string outDir = string.IsNullOrWhiteSpace(OutputDirectory)
                    ? Path.GetDirectoryName(CurrentPdfPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : OutputDirectory;

                StatusMessage = "Extraction des images en cours...";
                string imgDir = Path.Combine(outDir, $"{baseName}_Images");
                int count = await _extractionService.ExtractImagesToFolderAsync(CurrentPdfPath, imgDir, PageRange);
                StatusMessage = $"{count} image(s) extraite(s) avec succès dans : {imgDir}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de l'extraction : {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke();
    }
}
