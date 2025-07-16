using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace Librarian.Scheduler;

public partial class MainForm : Form
{
    private readonly BlazorWebView _blazorWebView;

    public MainForm()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhfdHVQRGJcWEZ3WkRWYEk=");
        InitializeComponent();
        
        _blazorWebView = new BlazorWebView()
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html",
            Services = Program.ServiceProvider
        };
        
        _blazorWebView.RootComponents.Add(new RootComponent("#app", typeof(Components.App), null));
        
        Controls.Add(_blazorWebView);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        
        // MainForm
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Librarian Scheduler";
        
        ResumeLayout(false);
    }
}