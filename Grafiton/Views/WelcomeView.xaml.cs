using System.IO;
using System.Windows;
using System.Windows.Controls;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class WelcomeView : UserControl
{
    public WelcomeView()
    {
        InitializeComponent();
    }

    private void UserControl_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && Path.GetExtension(files[0]).Equals(".pdf", System.StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }
        }
        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void UserControl_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && Path.GetExtension(files[0]).Equals(".pdf", System.StringComparison.OrdinalIgnoreCase))
            {
                if (DataContext is WelcomeViewModel vm && vm.OpenRecentFileCommand.CanExecute(null))
                {
                    if (Parent is ContentControl cc && cc.DataContext is MainViewModel mainVm)
                    {
                        mainVm.OpenPdfFile(files[0]);
                    }
                }
            }
        }
    }
}
