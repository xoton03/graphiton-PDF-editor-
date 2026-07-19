using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public class ColorOption
{
    public string Name { get; set; } = string.Empty;
    public string Hex { get; set; } = "#000000";
}

public partial class RedactionViewModel : ObservableObject
{
    private readonly IRedactionService _redactionService;

    [ObservableProperty]
    private string _currentPdfPath = string.Empty;

    [ObservableProperty]
    private int _targetPageIndex = 0;

    [ObservableProperty]
    private double _positionX = 50;

    [ObservableProperty]
    private double _positionY = 50;

    [ObservableProperty]
    private double _areaWidth = 200;

    [ObservableProperty]
    private double _areaHeight = 50;

    [ObservableProperty]
    private ColorOption _selectedColor;

    [ObservableProperty]
    private ObservableCollection<ColorOption> _availableColors = new()
    {
        new ColorOption { Name = "Noir (Masquage classique)", Hex = "#000000" },
        new ColorOption { Name = "Blanc (Effacement discret)", Hex = "#FFFFFF" },
        new ColorOption { Name = "Gris neutre", Hex = "#808080" },
        new ColorOption { Name = "Rouge (Avertissement)", Hex = "#FF0000" }
    };

    [ObservableProperty]
    private string _statusMessage = "Attention : le masquage définitif altère la page sans possibilité d'annulation.";

    public event Action? RequestClose;

    public RedactionViewModel(IRedactionService redactionService)
    {
        _redactionService = redactionService;
        _selectedColor = AvailableColors[0];
    }

    [RelayCommand]
    private async Task ApplyRedactionAsync()
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
            string outPath = Path.Combine(outDir, $"{baseName}_Masque.pdf");

            await _redactionService.RedactAreaAsync(
                CurrentPdfPath, TargetPageIndex, PositionX, PositionY, AreaWidth, AreaHeight, SelectedColor.Hex, outPath);

            StatusMessage = $"Masquage effectué avec succès dans : {Path.GetFileName(outPath)}";
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors du masquage : {ex.Message}";
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke();
    }
}
