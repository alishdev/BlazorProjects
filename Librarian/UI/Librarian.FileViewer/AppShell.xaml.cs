using Librarian.FileViewer.Components.Pages;

namespace Librarian.FileViewer;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(Components.Pages.FileViewer), typeof(Components.Pages.FileViewer));
	}
}
