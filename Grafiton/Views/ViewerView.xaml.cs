using System;
using System.Windows.Controls;
using Grafiton.ViewModels;

namespace Grafiton.Views;

public partial class ViewerView : UserControl
{
    public ViewerView()
    {
        InitializeComponent();
        InitializeWebView();

        DataContextChanged += ViewerView_DataContextChanged;
    }

    private void ViewerView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ViewerViewModel vm)
        {
            vm.PageManagerVM.PageSelected -= PageManagerVM_PageSelected;
            vm.PageManagerVM.PageSelected += PageManagerVM_PageSelected;
        }
    }

    private async void PageManagerVM_PageSelected(int pageNumber)
    {
        try
        {
            if (PdfWebView.CoreWebView2 != null)
            {
                await PdfWebView.CoreWebView2.ExecuteScriptAsync($"window.location.hash = 'page={pageNumber}';");
            }
        }
        catch { }
    }

    private async void InitializeWebView()
    {
        try
        {
            await PdfWebView.EnsureCoreWebView2Async();
            if (PdfWebView.CoreWebView2 != null)
            {
                PdfWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            }
        }
        catch { }
    }
}
