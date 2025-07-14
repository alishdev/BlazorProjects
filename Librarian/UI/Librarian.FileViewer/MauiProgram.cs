using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;
using Librarian.FileViewer.Services;

namespace Librarian.FileViewer;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhfdHVQRGJcWEZ3WkRWYEk=");
        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register Syncfusion license
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhfdHVQRGJcWEZ3WkRWYEk=");

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
