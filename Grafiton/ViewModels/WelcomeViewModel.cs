using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly Action<string> _openPdfCallback;

    [ObservableProperty]
    private ObservableCollection<RecentFile> _recentFiles = new();

    [ObservableProperty]
    private bool _hasRecentFiles;

    [ObservableProperty]
    private bool _hasNoRecentFiles = true;

    public WelcomeViewModel(IFileService fileService, Action<string> openPdfCallback)
    {
        _fileService = fileService;
        _openPdfCallback = openPdfCallback;
        LoadRecentFiles();
    }

    public void LoadRecentFiles()
    {
        var list = _fileService.GetRecentFiles();
        RecentFiles = new ObservableCollection<RecentFile>(list);
        UpdateRecentState();
    }

    private void UpdateRecentState()
    {
        HasRecentFiles = RecentFiles.Count > 0;
        HasNoRecentFiles = RecentFiles.Count == 0;
    }

    [RelayCommand]
    private void OpenPdf()
    {
        string? filePath = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(filePath))
        {
            _openPdfCallback(filePath);
        }
    }

    [RelayCommand]
    private void OpenRecentFile(RecentFile? recentFile)
    {
        if (recentFile != null && !string.IsNullOrEmpty(recentFile.FilePath))
        {
            _openPdfCallback(recentFile.FilePath);
        }
    }

    [RelayCommand]
    private void RemoveRecentFile(RecentFile? recentFile)
    {
        if (recentFile != null)
        {
            _fileService.RemoveRecentFile(recentFile.FilePath);
            RecentFiles.Remove(recentFile);
            UpdateRecentState();
        }
    }

    [RelayCommand]
    private void ClearRecentFiles()
    {
        _fileService.ClearRecentFiles();
        RecentFiles.Clear();
        UpdateRecentState();
    }
}
