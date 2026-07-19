using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class MergeSplitViewModel : ObservableObject
{
    private readonly IPdfEditService _pdfEditService;

    [ObservableProperty]
    private ObservableCollection<string> _filesToMerge = new();

    [ObservableProperty]
    private string? _selectedFileToMerge;

    [ObservableProperty]
    private string _splitSourceFile = string.Empty;

    [ObservableProperty]
    private string _pageRangeExpression = "1-3";

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public event Action? OperationCompleted;

    public MergeSplitViewModel(IPdfEditService pdfEditService)
    {
        _pdfEditService = pdfEditService;
    }

    [RelayCommand]
    private void AddFilesToMerge()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Fichiers PDF (*.pdf)|*.pdf",
            Multiselect = true,
            Title = "Sélectionner les fichiers PDF à fusionner"
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (string file in dialog.FileNames)
            {
                if (!FilesToMerge.Contains(file))
                {
                    FilesToMerge.Add(file);
                }
            }
        }
    }

    [RelayCommand]
    private void RemoveFileToMerge()
    {
        if (!string.IsNullOrEmpty(SelectedFileToMerge))
        {
            FilesToMerge.Remove(SelectedFileToMerge);
        }
    }

    [RelayCommand]
    private async Task ExecuteMergeAsync()
    {
        if (FilesToMerge.Count < 2)
        {
            StatusMessage = "Veuillez ajouter au moins 2 fichiers PDF à fusionner.";
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "Fichier PDF (*.pdf)|*.pdf",
            Title = "Enregistrer le PDF fusionné",
            FileName = "Document_Fusionne.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            IsProcessing = true;
            StatusMessage = "Fusion en cours...";

            try
            {
                await _pdfEditService.MergePdfFilesAsync(new(FilesToMerge), saveDialog.FileName);
                StatusMessage = "Fusion réussie !";
                OperationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la fusion : {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }

    [RelayCommand]
    private void SelectSplitSource()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Fichiers PDF (*.pdf)|*.pdf",
            Multiselect = false,
            Title = "Sélectionner le fichier PDF à scinder"
        };

        if (dialog.ShowDialog() == true)
        {
            SplitSourceFile = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task ExecuteSplitAsync()
    {
        if (string.IsNullOrEmpty(SplitSourceFile) || !File.Exists(SplitSourceFile))
        {
            StatusMessage = "Veuillez sélectionner un fichier PDF valide.";
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "Fichier PDF (*.pdf)|*.pdf",
            Title = "Enregistrer la sélection scindée",
            FileName = $"{Path.GetFileNameWithoutExtension(SplitSourceFile)}_Scinde.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            IsProcessing = true;
            StatusMessage = "Scission en cours...";

            try
            {
                await _pdfEditService.SplitPdfFileAsync(SplitSourceFile, PageRangeExpression, saveDialog.FileName);
                StatusMessage = "Scission réussie !";
                OperationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la scission : {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
