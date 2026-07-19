using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class RedactionDialog : Window
{
    public RedactionDialog(RedactionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += () => Close();
    }
}
