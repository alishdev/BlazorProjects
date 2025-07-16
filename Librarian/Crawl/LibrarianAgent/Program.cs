using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibrarianAgent;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<LibrarianBackgroundService>();
            })
            .UseSystemd(); // This will be ignored on macOS but allows for Linux compatibility

        var host = builder.Build();
        await host.RunAsync();
    }
}

public class LibrarianBackgroundService : BackgroundService
{
    private readonly ILogger<LibrarianBackgroundService> _logger;
    
    public LibrarianBackgroundService(ILogger<LibrarianBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Librarian Agent Service started at: {time}", DateTimeOffset.Now);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                // Add your background processing logic here
                _logger.LogInformation("Librarian Agent Service running at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Librarian Agent Service");
            throw;
        }
    }
} 