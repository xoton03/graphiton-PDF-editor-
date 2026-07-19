using System.Linq;
using System.Windows;
using Wpf.Ui.Controls;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class BatchDialog : FluentWindow
{
    public BatchDialog(BatchViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.RequestClose += Close;
    }

    private void OnBatchDragOver(object sender, DragEventArgs e)
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

    private void OnBatchDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files != null && DataContext is BatchViewModel vm)
            {
                vm.AddDroppedFiles(files);
            }
        }
    }
}
