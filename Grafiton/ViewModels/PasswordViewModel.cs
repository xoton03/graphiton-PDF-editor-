using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class PasswordViewModel : ObservableObject
{
    private readonly IPdfEditService _pdfEditService;
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public event Action? OperationCompleted;

    public PasswordViewModel(IPdfEditService pdfEditService)
    {
        _pdfEditService = pdfEditService;
    }

    public void Initialize(string filePath)
    {
        _currentFilePath = filePath;
    }

    [RelayCommand]
    private async Task ProtectPdfAsync()
    {
        if (string.IsNullOrEmpty(Password))
        {
            StatusMessage = "Veuillez saisir un mot de passe.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            StatusMessage = "Les mots de passe ne correspondent pas.";
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "Fichier PDF (*.pdf)|*.pdf",
            Title = "Enregistrer le PDF protégé",
            FileName = $"{Path.GetFileNameWithoutExtension(_currentFilePath)}_Protege.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                await _pdfEditService.ProtectPdfAsync(_currentFilePath, Password, Password, saveDialog.FileName);
                StatusMessage = "Protection appliquée avec succès !";
                OperationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
        }
    }
}
