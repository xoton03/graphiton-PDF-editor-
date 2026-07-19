using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class ComparisonView : Window
{
    public ComparisonView(ComparisonViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += () => Close();
    }
}
