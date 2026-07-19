using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class ConversionDialog : Window
{
    public ConversionDialog(ConversionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OperationCompleted += () => DialogResult = true;
    }
}
