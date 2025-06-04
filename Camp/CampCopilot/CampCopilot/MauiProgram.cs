using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace CampCopilot;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // Configure logging
        var executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var logsPath = Path.Combine(executablePath!, "Logs");
        System.Diagnostics.Debug.WriteLine($"Logs path: {logsPath}");
        Directory.CreateDirectory(logsPath);
        var logFile = Path.Combine(logsPath, "app.log");
        
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

#if WINDOWS
        builder.Services.AddTransient<IWebViewConfiguration, WebViewConfiguration>();
#endif

        builder.Logging.AddFile(logFile); // Simple file logging

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

#if WINDOWS
public class WebViewConfiguration : IWebViewConfiguration
{
    public void Configure(IWebView webView)
    {
        if (webView is Microsoft.Web.WebView2.Core.CoreWebView2 coreWebView)
        {
            coreWebView.Settings.IsWebMessageEnabled = true;
            coreWebView.Settings.AreDevToolsEnabled = true;
            coreWebView.Settings.AreDefaultContextMenusEnabled = true;
            coreWebView.Settings.IsStatusBarEnabled = true;
        }
    }
}
#endif