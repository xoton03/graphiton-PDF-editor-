using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class FormFillDialog : Window
{
    public FormFillDialog(FormFillViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += () => Close();
    }
}
