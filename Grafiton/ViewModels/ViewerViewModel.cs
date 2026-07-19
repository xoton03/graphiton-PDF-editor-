using System;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Grafiton.Models;
using Grafiton.Services;
using Grafiton.Views;

namespace Grafiton.ViewModels;

public partial class ViewerViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly IPdfRenderService _pdfRenderService;
    private readonly Action _closeCallback;
    private readonly Action<string> _openAnotherPdfCallback;

    [ObservableProperty]
    private PdfDocumentModel? _document;

    [ObservableProperty]
    private Uri? _sourceUri;

    [ObservableProperty]
    private string _title = "Document PDF";

    [ObservableProperty]
    private bool _isPagePanelOpen;

    public PageManagerViewModel PageManagerVM { get; }
    public AnnotationToolbarViewModel AnnotationToolbarVM { get; }
    public BookmarkPanelViewModel BookmarkPanelVM { get; }

    public ViewerViewModel(
        IFileService fileService,
        IPdfRenderService pdfRenderService,
        Action closeCallback,
        Action<string> openAnotherPdfCallback)
    {
        _fileService = fileService;
        _pdfRenderService = pdfRenderService;
        _closeCallback = closeCallback;
        _openAnotherPdfCallback = openAnotherPdfCallback;

        PageManagerVM = App.ServiceProvider.GetRequiredService<PageManagerViewModel>();
        AnnotationToolbarVM = App.ServiceProvider.GetRequiredService<AnnotationToolbarViewModel>();
        BookmarkPanelVM = App.ServiceProvider.GetRequiredService<BookmarkPanelViewModel>();

        PageManagerVM.DocumentModified += (newPath) =>
        {
            LoadDocument(newPath);
        };
    }

    public async void LoadDocument(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var fileInfo = new FileInfo(filePath);
        int pageCount = await _pdfRenderService.GetPageCountAsync(filePath);

        Document = new PdfDocumentModel
        {
            FilePath = filePath,
            FileName = fileInfo.Name,
            FileSizeBytes = fileInfo.Length,
            PageCount = pageCount,
            LastOpened = DateTime.Now
        };

        Title = Document.FileName;
        SourceUri = new Uri(filePath);
        _fileService.AddRecentFile(filePath);

        PageManagerVM.LoadDocument(filePath, pageCount);
        await BookmarkPanelVM.LoadBookmarksAsync(filePath);
    }

    [RelayCommand]
    private void TogglePagePanel()
    {
        IsPagePanelOpen = !IsPagePanelOpen;
    }

    [RelayCommand]
    private void ShowMergeSplitDialog()
    {
        var vm = App.ServiceProvider.GetRequiredService<MergeSplitViewModel>();
        var dialog = new MergeSplitDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void ShowWatermarkDialog()
    {
        if (Document == null) return;
        var vm = App.ServiceProvider.GetRequiredService<WatermarkViewModel>();
        vm.Initialize(Document.FilePath);
        var dialog = new WatermarkDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void ShowPasswordDialog()
    {
        if (Document == null) return;
        var vm = App.ServiceProvider.GetRequiredService<PasswordViewModel>();
        vm.Initialize(Document.FilePath);
        var dialog = new PasswordDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void ShowConversionDialog()
    {
        var vm = App.ServiceProvider.GetRequiredService<ConversionViewModel>();
        if (Document != null)
        {
            vm.Initialize(Document.FilePath);
        }
        var dialog = new ConversionDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void ShowBatchDialog()
    {
        var vm = App.ServiceProvider.GetRequiredService<BatchViewModel>();
        var dialog = new BatchDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void ShowSignatureDialog()
    {
        if (Document == null) return;
        var vm = App.ServiceProvider.GetRequiredService<SignatureViewModel>();
        vm.CurrentPdfPath = Document.FilePath;
        var dialog = new SignatureDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void CloseDocument()
    {
        _closeCallback();
    }

    [RelayCommand]
    private void OpenAnotherPdf()
    {
        string? filePath = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(filePath))
        {
            _openAnotherPdfCallback(filePath);
        }
    }
}
