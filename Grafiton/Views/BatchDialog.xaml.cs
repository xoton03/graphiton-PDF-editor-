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
}
