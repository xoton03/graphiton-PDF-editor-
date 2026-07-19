using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class SignatureViewModel : ObservableObject
{
    private readonly ISignatureService _signatureService;
    private readonly IFileService _fileService;

    [ObservableProperty]
    private string _currentPdfPath = string.Empty;

    [ObservableProperty]
    private string _signatureImagePath = string.Empty;

    [ObservableProperty]
    private int _targetPageIndex = 0;

    [ObservableProperty]
    private double _positionX = 100;

    [ObservableProperty]
    private double _positionY = 600;

    [ObservableProperty]
    private double _signatureWidth = 150;

    [ObservableProperty]
    private double _signatureHeight = 60;

    [ObservableProperty]
    private string _statusMessage = "Sélectionnez une image de signature PNG.";

    public event Action? RequestClose;

    public SignatureViewModel(ISignatureService signatureService, IFileService fileService)
    {
        _signatureService = signatureService;
        _fileService = fileService;
    }

    [RelayCommand]
    private void BrowseSignatureImage()
    {
        string? path = _fileService.OpenImageFileDialog();
        if (!string.IsNullOrEmpty(path))
        {
            SignatureImagePath = path;
            StatusMessage = $"Image sélectionnée : {Path.GetFileName(path)}";
        }
    }

    [RelayCommand]
    private async Task ApplySignatureAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPdfPath) || !File.Exists(CurrentPdfPath))
        {
            StatusMessage = "Aucun document PDF ouvert.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SignatureImagePath) || !File.Exists(SignatureImagePath))
        {
            StatusMessage = "Veuillez choisir une image de signature valide.";
            return;
        }

        try
        {
            string outDir = Path.GetDirectoryName(CurrentPdfPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string baseName = Path.GetFileNameWithoutExtension(CurrentPdfPath);
            string outPath = Path.Combine(outDir, $"{baseName}_Signe.pdf");

            await _signatureService.StampSignatureAsync(CurrentPdfPath, SignatureImagePath, TargetPageIndex, PositionX, PositionY, SignatureWidth, SignatureHeight, outPath);

            StatusMessage = $"Signature apposée avec succès dans : {Path.GetFileName(outPath)}";
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke();
    }
}
