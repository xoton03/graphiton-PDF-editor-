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

    // Helper boolean properties for XAML button styles & swatches
    [ObservableProperty]
    private bool _isHighlightSelected = true;

    [ObservableProperty]
    private bool _isNoteSelected;

    [ObservableProperty]
    private bool _isInkSelected;

    [ObservableProperty]
    private bool _isShapeSelected;

    public event Action<AnnotationType, string>? ConfigChanged;

    [RelayCommand]
    private void SelectTool(AnnotationType tool)
    {
        SelectedTool = tool;
        IsAnnotationActive = true;

        IsHighlightSelected = tool == AnnotationType.Highlight;
        IsNoteSelected = tool == AnnotationType.StickyNote;
        IsInkSelected = tool == AnnotationType.InkDrawing;
        IsShapeSelected = tool == AnnotationType.ShapeRectangle;

        ConfigChanged?.Invoke(SelectedTool, SelectedColor);
    }

    [RelayCommand]
    private void SetColor(string hexColor)
    {
        SelectedColor = hexColor;
        IsAnnotationActive = true;
        ConfigChanged?.Invoke(SelectedTool, SelectedColor);
    }

    [RelayCommand]
    private void ToggleActive()
    {
        IsAnnotationActive = !IsAnnotationActive;
    }
}
