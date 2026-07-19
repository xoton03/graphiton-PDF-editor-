using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class WatermarkDialog : Window
{
    public WatermarkDialog(WatermarkViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OperationCompleted += () => DialogResult = true;
    }
}
