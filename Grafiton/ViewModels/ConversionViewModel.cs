using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class ConversionViewModel : ObservableObject
{
    private readonly IConversionService _conversionService;
    private readonly ILibreOfficeService _libreOfficeService;
    private readonly ICompressionService _compressionService;
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private bool _isLibreOfficeInstalled;

    [ObservableProperty]
    private string _libreOfficeStatus = string.Empty;

    [ObservableProperty]
    private int _selectedDpi = 150;

    [ObservableProperty]
    private string _selectedImageFormat = "PNG";

    [ObservableProperty]
    private ObservableCollection<string> _imagesToConvert = new();

    [ObservableProperty]
    private string? _selectedImageToConvert;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private CompressionResult? _compressionResult;

    public event Action? OperationCompleted;

    public ConversionViewModel(
        IConversionService conversionService,
        ILibreOfficeService libreOfficeService,
        ICompressionService compressionService)
    {
        _conversionService = conversionService;
        _libreOfficeService = libreOfficeService;
        _compressionService = compressionService;

        CheckLibreOffice();
    }

    public void Initialize(string filePath)
    {
        _currentFilePath = filePath;
    }

    private void CheckLibreOffice()
    {
        IsLibreOfficeInstalled = _libreOfficeService.IsLibreOfficeInstalled(out string path);
        if (IsLibreOfficeInstalled)
        {
            LibreOfficeStatus = "LibreOffice est détecté et prêt pour les conversions bureautiques.";
        }
        else
        {
            LibreOfficeStatus = "LibreOffice n'est pas détecté. Seules les conversions d'images et la compression PDF sont disponibles.";
        }
    }

    [RelayCommand]
    private void OpenLibreOfficeDownloadSite()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://fr.libreoffice.org/telecharger/telecharger-libreoffice/",
                UseShellExecute = true
            });
        }
        catch { }
    }

    [RelayCommand]
    private async Task ExportPdfToImagesAsync()
    {
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath)) return;

        var dialog = new OpenFolderDialog
        {
            Title = "Sélectionner le dossier de destination des images"
        };

        if (dialog.ShowDialog() == true)
        {
            IsProcessing = true;
            StatusMessage = "Export des images en cours...";
            ProgressPercent = 0;

            try
            {
                var progress = new Progress<double>(p => ProgressPercent = p);
                var files = await _conversionService.ExportPdfToImagesAsync(_currentFilePath, SelectedImageFormat, SelectedDpi, dialog.FolderName, progress);
                StatusMessage = $"Export réussi ! {files.Count} image(s) enregistrée(s).";
                OperationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }

    [RelayCommand]
    private void AddImagesForPdf()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
            Multiselect = true,
            Title = "Sélectionner les images"
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (string file in dialog.FileNames)
            {
                if (!ImagesToConvert.Contains(file))
                {
                    ImagesToConvert.Add(file);
                }
            }
        }
    }

    [RelayCommand]
    private void RemoveImageForPdf()
    {
        if (!string.IsNullOrEmpty(SelectedImageToConvert))
        {
            ImagesToConvert.Remove(SelectedImageToConvert);
        }
    }

    [RelayCommand]
    private async Task ConvertImagesToPdfAsync()
    {
        if (ImagesToConvert.Count == 0)
        {
            StatusMessage = "Veuillez ajouter au moins une image.";
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "Fichier PDF (*.pdf)|*.pdf",
            Title = "Enregistrer le PDF généré",
            FileName = "Images_Converties.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            IsProcessing = true;
            StatusMessage = "Génération du PDF en cours...";

            try
            {
                var progress = new Progress<double>(p => ProgressPercent = p);
                await _conversionService.ConvertImagesToPdfAsync(new(ImagesToConvert), saveDialog.FileName, progress);
                StatusMessage = "Conversion des images en PDF réussie !";
                OperationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }

    [RelayCommand]
    private async Task ConvertPdfToOfficeAsync(string targetExtension)
    {
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath)) return;

        var dialog = new OpenFolderDialog
        {
            Title = $"Sélectionner le dossier de destination pour le fichier .{targetExtension}"
        };

        if (dialog.ShowDialog() == true)
        {
            IsProcessing = true;
            StatusMessage = $"Conversion en .{targetExtension} en cours via LibreOffice...";
            ProgressPercent = 20;

            try
            {
                var progress = new Progress<double>(p => ProgressPercent = p);
                string resultFile = await _conversionService.ConvertDocumentWithLibreOfficeAsync(_currentFilePath, targetExtension, dialog.FolderName, progress);
                StatusMessage = $"Conversion réussie ! Fichier : {Path.GetFileName(resultFile)}";
                OperationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }

    [RelayCommand]
    private async Task CompressPdfAsync()
    {
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath)) return;

        var saveDialog = new SaveFileDialog
        {
            Filter = "Fichier PDF (*.pdf)|*.pdf",
            Title = "Enregistrer le PDF optimisé",
            FileName = $"{Path.GetFileNameWithoutExtension(_currentFilePath)}_Compresse.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            IsProcessing = true;
            StatusMessage = "Optimisation et compression en cours...";

            try
            {
                CompressionResult = await _compressionService.CompressPdfAsync(_currentFilePath, CompressionQuality.Medium, saveDialog.FileName);
                StatusMessage = $"Compression réussie ! Taille réduite de {CompressionResult.ReductionFormatted}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
