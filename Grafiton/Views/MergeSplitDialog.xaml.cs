using System.Windows;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class MergeSplitDialog : Window
{
    public MergeSplitDialog(MergeSplitViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OperationCompleted += () => DialogResult = true;
    }
}
