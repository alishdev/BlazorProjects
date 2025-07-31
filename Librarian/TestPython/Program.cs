using CSnakes.Runtime;
using TestPython.Components;

namespace TestPython;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var home = Path.Join(Environment.CurrentDirectory, "."); /* Path to your Python modules */
        var venv = Path.Join(home, "venv");
        builder.Services.
            WithPython().
            WithHome(home).
            WithVirtualEnvironment(venv).
            FromRedistributable("3.12").
            //FromNuGet("3.12.4").
            //FromMacOSInstallerLocator("3.12").
            //FromEnvironmentVariable("PYTHON_HOME", "3.12").
            WithPipInstaller();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}