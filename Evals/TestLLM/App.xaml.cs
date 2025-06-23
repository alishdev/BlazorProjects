namespace TestLLM;

public partial class App : Application
{
    public App()
    {
        System.Diagnostics.Debug.WriteLine("=== APP CONSTRUCTOR START ===");
        InitializeComponent();
        
        // Initialize logging
        System.Diagnostics.Debug.WriteLine("About to initialize logging service...");
        LoggingService.Initialize();
        System.Diagnostics.Debug.WriteLine("Logging service initialization completed");
        LoggingService.LogInformation("TestLLM application starting");
        System.Diagnostics.Debug.WriteLine("=== APP CONSTRUCTOR END ===");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        LoggingService.LogInformation("Creating main window");
        return new Window(new AppShell());
    }
    
    protected override void OnStart()
    {
        base.OnStart();
        LoggingService.LogInformation("Application started");
    }
    
    protected override void OnSleep()
    {
        base.OnSleep();
        LoggingService.LogInformation("Application sleeping");
    }
    
    protected override void OnResume()
    {
        base.OnResume();
        LoggingService.LogInformation("Application resumed");
    }
}