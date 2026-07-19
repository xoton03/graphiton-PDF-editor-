using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Grafiton.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly IPdfRenderService _pdfRenderService;

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    [ObservableProperty]
    private bool _isDocumentOpen;

    [ObservableProperty]
    private string _windowTitle = "Grafiton — Lecteur PDF";

    [ObservableProperty]
    private ApplicationTheme _currentTheme = ApplicationTheme.Dark;

    [ObservableProperty]
    private SymbolRegular _themeIcon = SymbolRegular.DarkTheme24;

    public WelcomeViewModel WelcomeVM { get; }
    public ViewerViewModel ViewerVM { get; }

    public MainViewModel(IFileService fileService, IPdfRenderService pdfRenderService)
    {
        _fileService = fileService;
        _pdfRenderService = pdfRenderService;

        WelcomeVM = new WelcomeViewModel(_fileService, OpenPdfFile);
        ViewerVM = new ViewerViewModel(_fileService, _pdfRenderService, ShowWelcomeScreen, OpenPdfFile);

        CurrentViewModel = WelcomeVM;
        IsDocumentOpen = false;

        // Apply dark theme by default
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
    }

    public void OpenPdfFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        ViewerVM.LoadDocument(filePath);
        CurrentViewModel = ViewerVM;
        IsDocumentOpen = true;
        WindowTitle = $"Grafiton — {ViewerVM.Title}";
    }

    public void ShowWelcomeScreen()
    {
        WelcomeVM.LoadRecentFiles();
        CurrentViewModel = WelcomeVM;
        IsDocumentOpen = false;
        WindowTitle = "Grafiton — Lecteur PDF";
    }

    [RelayCommand]
    private void OpenPdf()
    {
        string? filePath = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(filePath))
        {
            OpenPdfFile(filePath);
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        if (CurrentTheme == ApplicationTheme.Dark)
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Light);
            CurrentTheme = ApplicationTheme.Light;
            ThemeIcon = SymbolRegular.WeatherMoon24;
        }
        else
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            CurrentTheme = ApplicationTheme.Dark;
            ThemeIcon = SymbolRegular.DarkTheme24;
        }
    }
}
