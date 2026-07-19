using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class HeaderFooterDialog : Window
{
    public HeaderFooterDialog(HeaderFooterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += () => Close();
    }
}
