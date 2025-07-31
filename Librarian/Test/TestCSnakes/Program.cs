using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestCSnakes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Add your services here
                    // Example: services.AddSingleton<IMyService, MyService>();
                    var home = Path.Join(Environment.CurrentDirectory, "."); /* Path to your Python modules */
                    var venv = Path.Join(home, "venv");
                    services.
                        WithPython().
                        WithHome(home).
                        WithVirtualEnvironment(venv).
                        FromRedistributable("3.12").
                        //FromNuGet("3.12.4").
                        FromMacOSInstallerLocator("3.12").
                        //FromEnvironmentVariable("PYTHON_HOME", "3.12").
                        WithPipInstaller();
                });

            var host = builder.Build();

            Console.WriteLine("Hello, World!");

            var env = host.Services.GetRequiredService<IPythonEnvironment>();
            var demo = env.Test();
            var coolThings = demo.CoolThings("a");
            foreach (var thing in coolThings)
            {
                Console.WriteLine(thing);
            }

        }
    }
}
