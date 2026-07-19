using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class FormFillViewModel : ObservableObject
{
    private readonly IFormFillService _formFillService;

    [ObservableProperty]
    private string _currentPdfPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<FormItemModel> _fields = new();

    [ObservableProperty]
    private bool _flattenForm = false;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Chargement des champs de formulaire...";

    public event Action? RequestClose;

    public FormFillViewModel(IFormFillService formFillService)
    {
        _formFillService = formFillService;
    }

    public async Task LoadFormFieldsAsync(string pdfPath)
    {
        CurrentPdfPath = pdfPath;
        IsLoading = true;
        Fields.Clear();

        try
        {
            var loadedFields = await _formFillService.GetFormFieldsAsync(pdfPath);
            foreach (var f in loadedFields)
            {
                Fields.Add(f);
            }

            StatusMessage = Fields.Count > 0
                ? $"{Fields.Count} champ(s) de formulaire détecté(s)."
                : "Aucun champ de formulaire AcroForm détecté dans ce document.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur de chargement : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveFormAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPdfPath) || !File.Exists(CurrentPdfPath))
        {
            StatusMessage = "Aucun document PDF valide.";
            return;
        }

        try
        {
            string outDir = Path.GetDirectoryName(CurrentPdfPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string baseName = Path.GetFileNameWithoutExtension(CurrentPdfPath);
            string outPath = Path.Combine(outDir, $"{baseName}_Rempli.pdf");

            var fieldList = new System.Collections.Generic.List<FormItemModel>(Fields);
            await _formFillService.FillFormFieldsAsync(CurrentPdfPath, fieldList, FlattenForm, outPath);

            StatusMessage = $"Formulaire sauvegardé dans : {Path.GetFileName(outPath)}";
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de la sauvegarde : {ex.Message}";
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke();
    }
}
