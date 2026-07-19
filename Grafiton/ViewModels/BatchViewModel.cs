using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class BatchViewModel : ObservableObject
{
    private readonly IBatchService _batchService;
    private readonly IFileService _fileService;

    [ObservableProperty]
    private ObservableCollection<BatchJob> _jobs = new();

    [ObservableProperty]
    private BatchOperationType _selectedOperation = BatchOperationType.Watermark;

    [ObservableProperty]
    private string _watermarkText = "CONFIDENTIAL";

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private double _overallProgress;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = "Prêt à démarrer la file d'attente.";

    public event Action? RequestClose;

    public BatchViewModel(IBatchService batchService, IFileService fileService)
    {
        _batchService = batchService;
        _fileService = fileService;
    }

    [RelayCommand]
    private void AddFiles()
    {
        var files = _fileService.OpenMultipleFilesDialog();
        if (files != null && files.Length > 0)
        {
            foreach (var f in files)
            {
                Jobs.Add(new BatchJob
                {
                    SourceFilePath = f,
                    Operation = SelectedOperation,
                    WatermarkText = WatermarkText,
                    Password = Password
                });
            }
        }
    }

    [RelayCommand]
    private void RemoveJob(BatchJob job)
    {
        if (job != null) Jobs.Remove(job);
    }

    [RelayCommand]
    private void ClearJobs()
    {
        Jobs.Clear();
    }

    [RelayCommand]
    private async Task StartBatchAsync()
    {
        if (Jobs.Count == 0)
        {
            StatusMessage = "Veuillez ajouter au moins un fichier PDF.";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Traitement par lot en cours...";

        // Update jobs with selected settings
        foreach (var j in Jobs)
        {
            j.Operation = SelectedOperation;
            j.WatermarkText = WatermarkText;
            j.Password = Password;
        }

        var progress = new Progress<double>(val => OverallProgress = val);
        await _batchService.ProcessBatchAsync(new System.Collections.Generic.List<BatchJob>(Jobs), progress);

        IsProcessing = false;
        StatusMessage = "Traitement par lot terminé !";
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke();
    }
}
