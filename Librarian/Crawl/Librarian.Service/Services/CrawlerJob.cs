using Librarian.Core;
using Librarian.Service.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Collections.Concurrent;
using System.Reflection;

namespace Librarian.Service.Services
{
    public class CrawlerJob : IJob
    {
        private readonly ILogger<CrawlerJob> _logger;
        private static readonly ConcurrentDictionary<string, object> _runningJobs = new();

        public CrawlerJob(ILogger<CrawlerJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobData = context.JobDetail.JobDataMap;
            var config = (ScheduledJobConfig)jobData["config"];

            var jobKey = $"{config.CrawlerType}_{config.Parameter?.ToString() ?? "null"}";

            if (_runningJobs.ContainsKey(jobKey))
            {
                _logger.LogWarning("Job {JobKey} is already running, skipping execution", jobKey);
                return;
            }

            _runningJobs[jobKey] = new object();

            try
            {
                _logger.LogInformation("Starting job {JobKey}", jobKey);

                var assembly = Assembly.LoadFrom(config.CrawlerAssembly);
                var type = assembly.GetType(config.CrawlerType);

                if (type == null)
                {
                    _logger.LogError("Type {CrawlerType} not found in assembly {CrawlerAssembly}", 
                        config.CrawlerType, config.CrawlerAssembly);
                    return;
                }

                if (!typeof(ICrawler).IsAssignableFrom(type))
                {
                    _logger.LogError("Type {CrawlerType} does not implement ICrawler interface", 
                        config.CrawlerType);
                    return;
                }

                var crawler = Activator.CreateInstance(type) as ICrawler;
                if (crawler == null)
                {
                    _logger.LogError("Failed to create instance of {CrawlerType}", config.CrawlerType);
                    return;
                }

                await Task.Run(() => crawler.Run(config.Parameter));

                _logger.LogInformation("Job {JobKey} completed successfully", jobKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job {JobKey} failed with error: {Error}", jobKey, ex.Message);
            }
            finally
            {
                _runningJobs.TryRemove(jobKey, out _);
            }
        }
    }
}