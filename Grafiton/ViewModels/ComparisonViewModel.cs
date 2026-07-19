using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class ComparisonViewModel : ObservableObject
{
    private readonly IComparisonService _comparisonService;
    private readonly IFileService _fileService;

    [ObservableProperty]
    private string _originalPdfPath = string.Empty;

    [ObservableProperty]
    private string _modifiedPdfPath = string.Empty;

    [ObservableProperty]
    private ComparisonResultModel? _comparisonResult;

    [ObservableProperty]
    private PageComparisonResult? _selectedPageResult;

    [ObservableProperty]
    private bool _isComparing;

    [ObservableProperty]
    private string _statusMessage = "Sélectionnez deux fichiers PDF à comparer.";

    public event Action? RequestClose;

    public ComparisonViewModel(IComparisonService comparisonService, IFileService fileService)
    {
        _comparisonService = comparisonService;
        _fileService = fileService;
    }

    [RelayCommand]
    private void BrowseOriginal()
    {
        string? file = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(file))
        {
            OriginalPdfPath = file;
        }
    }

    [RelayCommand]
    private void BrowseModified()
    {
        string? file = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(file))
        {
            ModifiedPdfPath = file;
        }
    }

    [RelayCommand]
    private async Task StartComparisonAsync()
    {
        if (string.IsNullOrWhiteSpace(OriginalPdfPath) || !File.Exists(OriginalPdfPath))
        {
            StatusMessage = "Veuillez sélectionner un premier fichier PDF valide.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ModifiedPdfPath) || !File.Exists(ModifiedPdfPath))
        {
            StatusMessage = "Veuillez sélectionner un second fichier PDF valide.";
            return;
        }

        IsComparing = true;
        StatusMessage = "Comparaison en cours... Rendu de la superposition d'images...";

        try
        {
            ComparisonResult = await _comparisonService.CompareDocumentsAsync(OriginalPdfPath, ModifiedPdfPath);

            if (ComparisonResult.Pages.Count > 0)
            {
                SelectedPageResult = ComparisonResult.Pages[0];
            }

            StatusMessage = ComparisonResult.TotalPagesWithDiffs > 0
                ? $"{ComparisonResult.TotalPagesWithDiffs} page(s) contenant des différences visuelles (surlignées en magenta)."
                : "Les deux documents sont visuellement identiques !";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de la comparaison : {ex.Message}";
        }
        finally
        {
            IsComparing = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke();
    }
}
