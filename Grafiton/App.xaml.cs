using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Grafiton.Services;
using Grafiton.ViewModels;

namespace Grafiton;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Services
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IPdfRenderService, PdfRenderService>();
        services.AddSingleton<IPdfEditService, PdfEditService>();
        services.AddSingleton<IAnnotationService, AnnotationService>();
        services.AddSingleton<ILibreOfficeService, LibreOfficeService>();
        services.AddSingleton<IConversionService, ConversionService>();
        services.AddSingleton<ICompressionService, CompressionService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<PageManagerViewModel>();
        services.AddTransient<AnnotationToolbarViewModel>();
        services.AddTransient<MergeSplitViewModel>();
        services.AddTransient<WatermarkViewModel>();
        services.AddTransient<PasswordViewModel>();
        services.AddTransient<ConversionViewModel>();

        // Views
        services.AddSingleton<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // If a file path argument was passed via command line
        if (e.Args.Length > 0 && System.IO.File.Exists(e.Args[0]))
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel.OpenPdfFile(e.Args[0]);
        }
    }
}
