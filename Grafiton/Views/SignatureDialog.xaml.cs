using Wpf.Ui.Controls;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class SignatureDialog : FluentWindow
{
    public SignatureDialog(SignatureViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.RequestClose += Close;
    }
}
