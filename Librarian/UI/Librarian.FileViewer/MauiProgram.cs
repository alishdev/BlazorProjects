using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;
using Syncfusion.Maui.PdfViewer;
using Librarian.FileViewer.Services;
using Syncfusion.Maui.Core.Hosting;

namespace Librarian.FileViewer;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("REMOVED_SYNCFUSION_LICENSE_KEY=");
        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureSyncfusionCore();

		// Register Syncfusion license
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("REMOVED_SYNCFUSION_LICENSE_KEY=");

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSyncfusionBlazor();
		builder.Services.AddSingleton<FileHierarchyService>();
		builder.Services.AddSingleton<FileContentService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
