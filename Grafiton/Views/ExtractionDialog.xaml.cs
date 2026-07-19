using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class ExtractionDialog : Window
{
    public ExtractionDialog(ExtractionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += () => Close();
    }
}
