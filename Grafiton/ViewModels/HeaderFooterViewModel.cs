using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class HeaderFooterViewModel : ObservableObject
{
    private readonly IHeaderFooterService _headerFooterService;

    [ObservableProperty]
    private string _currentPdfPath = string.Empty;

    [ObservableProperty]
    private string _headerText = string.Empty;

    [ObservableProperty]
    private string _footerText = string.Empty;

    [ObservableProperty]
    private bool _includePageNumbers = true;

    [ObservableProperty]
    private PageNumberPosition _numberPosition = PageNumberPosition.BottomCenter;

    [ObservableProperty]
    private string _pageNumberFormat = "Page {0} sur {1}";

    [ObservableProperty]
    private double _fontSize = 10.0;

    [ObservableProperty]
    private string _pageRange = "all";

    [ObservableProperty]
    private string _statusMessage = "Personnalisez vos en-têtes, pieds de page et numéros de page.";

    public event Action? RequestClose;

    public HeaderFooterViewModel(IHeaderFooterService headerFooterService)
    {
        _headerFooterService = headerFooterService;
    }

    [RelayCommand]
    private async Task ApplyHeaderFooterAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPdfPath) || !File.Exists(CurrentPdfPath))
        {
            StatusMessage = "Aucun fichier PDF valide sélectionné.";
            return;
        }

        try
        {
            string outDir = Path.GetDirectoryName(CurrentPdfPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string baseName = Path.GetFileNameWithoutExtension(CurrentPdfPath);
            string outPath = Path.Combine(outDir, $"{baseName}_Pagines.pdf");

            var opts = new HeaderFooterOptions
            {
                HeaderText = HeaderText,
                FooterText = FooterText,
                IncludePageNumbers = IncludePageNumbers,
                NumberPosition = NumberPosition,
                PageNumberFormat = PageNumberFormat,
                FontSize = FontSize,
                PageRange = PageRange
            };

            await _headerFooterService.AddHeaderFooterAsync(CurrentPdfPath, opts, outPath);

            StatusMessage = $"En-têtes et numéros de page ajoutés avec succès dans : {Path.GetFileName(outPath)}";
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
