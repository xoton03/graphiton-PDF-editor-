using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class PasswordDialog : Window
{
    public PasswordDialog(PasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OperationCompleted += () => DialogResult = true;
    }
}
