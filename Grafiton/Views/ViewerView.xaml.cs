using System;
using System.Windows.Controls;
using Grafiton.Models;
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

            vm.AnnotationToolbarVM.ConfigChanged -= AnnotationToolbarVM_ConfigChanged;
            vm.AnnotationToolbarVM.ConfigChanged += AnnotationToolbarVM_ConfigChanged;
        }
    }

    private async void AnnotationToolbarVM_ConfigChanged(AnnotationType tool, string hexColor)
    {
        try
        {
            if (PdfWebView.CoreWebView2 != null)
            {
                string js = @"
                    (function() {
                        try {
                            const viewer = document.querySelector('pdf-viewer') || document.querySelector('embed');
                            if (viewer) {
                                viewer.postMessage({
                                    type: 'setAnnotationMode',
                                    tool: '" + tool.ToString().ToLower() + @"',
                                    color: '" + hexColor + @"'
                                }, '*');
                            }
                        } catch(e) {}
                    })();";
                await PdfWebView.CoreWebView2.ExecuteScriptAsync(js);
            }
        }
        catch { }
    }

    private async void PageManagerVM_PageSelected(int pageNumber)
    {
        try
        {
            if (PdfWebView.CoreWebView2 != null)
            {
                await PdfWebView.CoreWebView2.ExecuteScriptAsync("window.location.hash = 'page=" + pageNumber + "';");
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
                PdfWebView.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = true;
            }
        }
        catch { }
    }
}
