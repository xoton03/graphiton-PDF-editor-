using System.Linq;
using System.Windows;
using Wpf.Ui.Controls;
using Grafiton.ViewModels;

namespace Grafiton;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnWindowPreviewDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Any(f => f.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }
        }
        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void OnWindowDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            string? firstPdf = files?.FirstOrDefault(f => f.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(firstPdf) && DataContext is MainViewModel vm)
            {
                vm.OpenPdfFile(firstPdf);
            }
        }
    }
}