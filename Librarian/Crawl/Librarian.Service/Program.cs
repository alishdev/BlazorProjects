using Librarian.Service.Services;
using Quartz;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/librarian-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddQuartz(q =>
{
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 10;
    });
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddHostedService<CrawlerSchedulerService>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Librarian Crawler Service";
});

var host = builder.Build();

try
{
    Log.Information("Starting Librarian Crawler Service");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Librarian Crawler Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}