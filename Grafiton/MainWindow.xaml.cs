using Wpf.Ui.Controls;
using Grafiton.ViewModels;

namespace Grafiton;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}