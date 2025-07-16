using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Librarian.Scheduler.Services;
using Syncfusion.Blazor;

namespace Librarian.Scheduler;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("REMOVED_SYNCFUSION_LICENSE_KEY=");
                                                                        

        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        Application.Run(new MainForm());
    }

    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddWindowsFormsBlazorWebView();
                services.AddSyncfusionBlazor();
                services.AddSingleton<ConfigurationService>();
#if DEBUG
                services.AddBlazorWebViewDeveloperTools();
#endif
            });
    }
}