using Librarian.Core;
using Librarian.Scheduler.Models;
using System.Reflection;
using System.Text.Json;

namespace Librarian.Scheduler.Services
{
    public class ConfigurationService
    {
        private readonly string _appsettingsPath;
        private readonly string _crawlersPath;

        public ConfigurationService()
        {
            // For MAUI apps, we'll use platform-specific locations
            var appDataPath = GetPlatformAppDataPath();
            _appsettingsPath = Path.Combine(appDataPath, "appsettings.json");
            _crawlersPath = Path.Combine(appDataPath, "crawlers");
            
            // Ensure directories exist
            Directory.CreateDirectory(Path.GetDirectoryName(_appsettingsPath)!);
            Directory.CreateDirectory(_crawlersPath);
        }

        private string GetPlatformAppDataPath()
        {
#if WINDOWS
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Librarian");
#elif MACCATALYST
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Librarian");
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Librarian");
#endif
        }

        public async Task<List<ScheduledJobModel>> GetScheduledJobsAsync()
        {
            try
            {
                if (!File.Exists(_appsettingsPath))
                {
                    return new List<ScheduledJobModel>();
                }

                var json = await File.ReadAllTextAsync(_appsettingsPath);
                var doc = JsonDocument.Parse(json);
                
                if (!doc.RootElement.TryGetProperty("ScheduledJobs", out var jobsElement))
                {
                    return new List<ScheduledJobModel>();
                }

                var jobs = new List<ScheduledJobModel>();
                var id = 1;

                foreach (var jobElement in jobsElement.EnumerateArray())
                {
                    var job = new ScheduledJobModel
                    {
                        Id = id++,
                        IsEnabled = jobElement.GetProperty("IsEnabled").GetBoolean(),
                        Schedule = jobElement.GetProperty("Schedule").GetString() ?? "",
                        CrawlerAssembly = jobElement.GetProperty("CrawlerAssembly").GetString() ?? "",
                        CrawlerType = jobElement.GetProperty("CrawlerType").GetString() ?? "",
                        Parameter = jobElement.TryGetProperty("Parameter", out var param) ? param.ToString() : ""
                    };

                    job.ScheduleDisplay = GetScheduleDisplay(job.Schedule);
                    job.CrawlerTypeDisplay = job.CrawlerType.Split('.').LastOrDefault() ?? job.CrawlerType;

                    jobs.Add(job);
                }

                return jobs;
            }
            catch (Exception)
            {
                return new List<ScheduledJobModel>();
            }
        }

        public async Task SaveScheduledJobsAsync(List<ScheduledJobModel> jobs)
        {
            try
            {
                var existingJson = "{}";
                if (File.Exists(_appsettingsPath))
                {
                    existingJson = await File.ReadAllTextAsync(_appsettingsPath);
                }

                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(existingJson);
                }
                catch
                {
                    doc = JsonDocument.Parse("{}");
                }
                
                var root = doc.RootElement.Clone();

                var jobsArray = jobs.Select(job => new
                {
                    IsEnabled = job.IsEnabled,
                    Schedule = job.Schedule,
                    CrawlerAssembly = job.CrawlerAssembly,
                    CrawlerType = job.CrawlerType,
                    Parameter = string.IsNullOrWhiteSpace(job.Parameter) ? null : 
                        (job.Parameter.StartsWith("{") ? JsonSerializer.Deserialize<object>(job.Parameter) : job.Parameter)
                }).ToArray();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var newConfig = new Dictionary<string, object>();

                // Copy existing configuration
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Name != "ScheduledJobs")
                    {
                        newConfig[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText()) ?? new object();
                    }
                }

                // Add default Serilog configuration if not present
                if (!newConfig.ContainsKey("Serilog"))
                {
                    newConfig["Serilog"] = new
                    {
                        Using = new[] { "Serilog.Sinks.Console", "Serilog.Sinks.File" },
                        MinimumLevel = new
                        {
                            Default = "Information",
                            Override = new
                            {
                                Microsoft = "Warning",
                                System = "Warning",
                                Quartz = "Information"
                            }
                        },
                        WriteTo = new object[]
                        {
                            new
                            {
                                Name = "Console",
                                Args = new
                                {
                                    outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
                                }
                            },
                            new
                            {
                                Name = "File",
                                Args = new
                                {
                                    path = "logs/librarian-.log",
                                    rollingInterval = "Day",
                                    retainedFileCountLimit = 30,
                                    outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
                                }
                            }
                        }
                    };
                }

                newConfig["ScheduledJobs"] = jobsArray;

                var json = JsonSerializer.Serialize(newConfig, options);
                await File.WriteAllTextAsync(_appsettingsPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        public List<CrawlerInfo> GetAvailableCrawlers()
        {
            var crawlers = new List<CrawlerInfo>();

            try
            {
                var dllFiles = Directory.GetFiles(_crawlersPath, "*.dll", SearchOption.TopDirectoryOnly);

                foreach (var dllFile in dllFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(dllFile);
                        var crawlerTypes = assembly.GetTypes()
                            .Where(t => t.IsPublic && !t.IsAbstract && typeof(ICrawler).IsAssignableFrom(t))
                            .ToList();

                        foreach (var type in crawlerTypes)
                        {
                            crawlers.Add(new CrawlerInfo
                            {
                                AssemblyName = Path.GetFileName(dllFile),
                                TypeName = type.FullName ?? type.Name,
                                DisplayName = type.Name
                            });
                        }
                    }
                    catch
                    {
                        // Skip assemblies that can't be loaded
                    }
                }
            }
            catch
            {
                // Return empty list if directory doesn't exist or can't be accessed
            }

            // Add a default crawler for demonstration if no crawlers found
            if (crawlers.Count == 0)
            {
                crawlers.Add(new CrawlerInfo
                {
                    AssemblyName = "FileCrawler.dll",
                    TypeName = "FileCrawler.FileCrawler",
                    DisplayName = "FileCrawler"
                });
            }

            return crawlers;
        }

        public List<ScheduleOption> GetScheduleOptions()
        {
            return new List<ScheduleOption>
            {
                new() { Display = "Every Minute", CronExpression = "0 * * * * ?" },
                new() { Display = "Every 5 Minutes", CronExpression = "0 */5 * * * ?" },
                new() { Display = "Every 10 Minutes", CronExpression = "0 */10 * * * ?" },
                new() { Display = "Every 15 Minutes", CronExpression = "0 */15 * * * ?" },
                new() { Display = "Every 30 Minutes", CronExpression = "0 */30 * * * ?" },
                new() { Display = "Every Hour", CronExpression = "0 0 * * * ?" },
                new() { Display = "Every 2 Hours", CronExpression = "0 0 */2 * * ?" },
                new() { Display = "Every 6 Hours", CronExpression = "0 0 */6 * * ?" },
                new() { Display = "Daily at Midnight", CronExpression = "0 0 0 * * ?" },
                new() { Display = "Daily at 6 AM", CronExpression = "0 0 6 * * ?" },
                new() { Display = "Daily at 12 PM", CronExpression = "0 0 12 * * ?" },
                new() { Display = "Daily at 6 PM", CronExpression = "0 0 18 * * ?" },
                new() { Display = "Weekly (Sundays at Midnight)", CronExpression = "0 0 0 ? * SUN" },
                new() { Display = "Monthly (1st at Midnight)", CronExpression = "0 0 0 1 * ?" },
                new() { Display = "Custom", CronExpression = "" }
            };
        }

        private string GetScheduleDisplay(string cronExpression)
        {
            var scheduleOptions = GetScheduleOptions();
            var option = scheduleOptions.FirstOrDefault(s => s.CronExpression == cronExpression);
            return option?.Display ?? $"Custom: {cronExpression}";
        }
    }
}