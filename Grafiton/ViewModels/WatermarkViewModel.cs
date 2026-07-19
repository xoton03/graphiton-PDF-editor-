using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class WatermarkViewModel : ObservableObject
{
    private readonly IPdfEditService _pdfEditService;
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private WatermarkOptions _options = new();

    [ObservableProperty]
    private string _watermarkText = "CONFIDENTIEL";

    [ObservableProperty]
    private double _opacity = 0.3;

    [ObservableProperty]
    private double _rotationAngle = -45.0;

    [ObservableProperty]
    private string _selectedColor = "#808080";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public int OpacityPercent => (int)Math.Round(Opacity * 100);
    public int FormattedAngle => (int)Math.Round(RotationAngle);

    public event Action? OperationCompleted;

    public WatermarkViewModel(IPdfEditService pdfEditService)
    {
        _pdfEditService = pdfEditService;
    }

    partial void OnOpacityChanged(double value)
    {
        OnPropertyChanged(nameof(OpacityPercent));
    }

    partial void OnRotationAngleChanged(double value)
    {
        OnPropertyChanged(nameof(FormattedAngle));
    }

    public void Initialize(string filePath)
    {
        _currentFilePath = filePath;
    }

    [RelayCommand]
    private async Task ApplyWatermarkAsync()
    {
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath)) return;

        var saveDialog = new SaveFileDialog
        {
            Filter = "Fichier PDF (*.pdf)|*.pdf",
            Title = "Enregistrer le PDF avec filigrane",
            FileName = $"{Path.GetFileNameWithoutExtension(_currentFilePath)}_Filigrane.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                Options.Text = WatermarkText;
                Options.Opacity = Opacity;
                Options.RotationAngle = RotationAngle;
                Options.HexColor = SelectedColor;

                await _pdfEditService.AddTextWatermarkAsync(_currentFilePath, Options, saveDialog.FileName);
                StatusMessage = "Filigrane appliqué avec succès !";
                OperationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
        }
    }
}
