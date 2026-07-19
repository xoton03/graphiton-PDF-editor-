using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;

namespace Grafiton.ViewModels;

public partial class AnnotationToolbarViewModel : ObservableObject
{
    [ObservableProperty]
    private AnnotationType _selectedTool = AnnotationType.Highlight;

    [ObservableProperty]
    private bool _isAnnotationActive;

    [ObservableProperty]
    private string _selectedColor = "#FFD700"; // Yellow default

    [ObservableProperty]
    private double _strokeThickness = 3.0;

    public event Action<AnnotationType, string>? ConfigChanged;

    [RelayCommand]
    private void SelectTool(AnnotationType tool)
    {
        SelectedTool = tool;
        IsAnnotationActive = true;
        ConfigChanged?.Invoke(SelectedTool, SelectedColor);
    }

    [RelayCommand]
    private void SetColor(string hexColor)
    {
        SelectedColor = hexColor;
        ConfigChanged?.Invoke(SelectedTool, SelectedColor);
    }

    [RelayCommand]
    private void ToggleActive()
    {
        IsAnnotationActive = !IsAnnotationActive;
    }
}
