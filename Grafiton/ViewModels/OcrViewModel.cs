using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class OcrViewModel : ObservableObject
{
    private readonly IOcrService _ocrService;

    [ObservableProperty]
    private string _currentPdfPath = string.Empty;

    [ObservableProperty]
    private string _recognizedText = string.Empty;

    [ObservableProperty]
    private bool _isRecognizing;

    [ObservableProperty]
    private string _selectedLanguage = "fr-FR";

    [ObservableProperty]
    private string _statusMessage = "Prêt pour l'analyse OCR.";

    public OcrViewModel(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    [RelayCommand]
    private async Task StartOcrAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPdfPath) || !File.Exists(CurrentPdfPath))
        {
            StatusMessage = "Aucun document PDF valide sélectionné.";
            return;
        }

        IsRecognizing = true;
        StatusMessage = "Reconnaissance optique de texte en cours...";

        try
        {
            RecognizedText = await _ocrService.RecognizeDocumentTextAsync(CurrentPdfPath, SelectedLanguage);
            StatusMessage = "Reconnaissance de texte terminée !";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur OCR : {ex.Message}";
        }

        IsRecognizing = false;
    }
}
