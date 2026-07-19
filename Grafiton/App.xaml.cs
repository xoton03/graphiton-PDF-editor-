using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Grafiton.Services;
using Grafiton.ViewModels;

namespace Grafiton;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        PdfSharpCore.Fonts.GlobalFontSettings.FontResolver = new FontResolver();

        RegisterPdfAssociation();

        var services = new ServiceCollection();

        // Services
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IPdfRenderService, PdfRenderService>();
        services.AddSingleton<IPdfEditService, PdfEditService>();
        services.AddSingleton<IAnnotationService, AnnotationService>();
        services.AddSingleton<ILibreOfficeService, LibreOfficeService>();
        services.AddSingleton<IConversionService, ConversionService>();
        services.AddSingleton<ICompressionService, CompressionService>();
        services.AddSingleton<IBatchService, BatchService>();
        services.AddSingleton<IOcrService, OcrService>();
        services.AddSingleton<ISignatureService, SignatureService>();
        services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
        services.AddSingleton<IBookmarkService, BookmarkService>();
        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IExtractionService, ExtractionService>();
        services.AddSingleton<IFormFillService, FormFillService>();
        services.AddSingleton<IRedactionService, RedactionService>();
        services.AddSingleton<IComparisonService, ComparisonService>();
        services.AddSingleton<IHeaderFooterService, HeaderFooterService>();
        services.AddSingleton<IPdfAExportService, PdfAExportService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<PageManagerViewModel>();
        services.AddTransient<AnnotationToolbarViewModel>();
        services.AddTransient<MergeSplitViewModel>();
        services.AddTransient<WatermarkViewModel>();
        services.AddTransient<PasswordViewModel>();
        services.AddTransient<ConversionViewModel>();
        services.AddTransient<BatchViewModel>();
        services.AddTransient<SignatureViewModel>();
        services.AddTransient<OcrViewModel>();
        services.AddTransient<BookmarkPanelViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<TextToSpeechViewModel>();
        services.AddTransient<ExtractionViewModel>();
        services.AddTransient<FormFillViewModel>();
        services.AddTransient<RedactionViewModel>();
        services.AddTransient<ComparisonViewModel>();
        services.AddTransient<HeaderFooterViewModel>();

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

    private static void RegisterPdfAssociation()
    {
        try
        {
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (string.IsNullOrEmpty(exePath) || !System.IO.File.Exists(exePath)) return;

            string progId = "Grafiton.Document";

            using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
            {
                key.SetValue("", "Document PDF Grafiton");
            }

            using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}\DefaultIcon"))
            {
                key.SetValue("", $"\"{exePath}\",0");
            }

            using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}\shell\open\command"))
            {
                key.SetValue("", $"\"{exePath}\" \"%1\"");
            }

            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.pdf"))
            {
                key.SetValue("", progId);
            }

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }
        catch { }
    }
}
