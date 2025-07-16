using Librarian.Service.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Librarian.Service.Services
{
    public class CrawlerSchedulerService : BackgroundService
    {
        private readonly ILogger<CrawlerSchedulerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IScheduler _scheduler;

        public CrawlerSchedulerService(
            ILogger<CrawlerSchedulerService> logger,
            IConfiguration configuration,
            IScheduler scheduler)
        {
            _logger = logger;
            _configuration = configuration;
            _scheduler = scheduler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Crawler Scheduler Service is starting");

            await _scheduler.Start(stoppingToken);

            await ScheduleJobs();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            await _scheduler.Shutdown(stoppingToken);
        }

        private async Task ScheduleJobs()
        {
            var scheduledJobs = _configuration.GetSection("ScheduledJobs").Get<ScheduledJobConfig[]>();

            if (scheduledJobs == null || scheduledJobs.Length == 0)
            {
                _logger.LogWarning("No scheduled jobs found in configuration");
                return;
            }

            foreach (var jobConfig in scheduledJobs.Where(j => j.IsEnabled))
            {
                try
                {
                    var jobKey = new JobKey($"{jobConfig.CrawlerType}_{Guid.NewGuid()}", "CrawlerJobs");
                    
                    var job = JobBuilder.Create<CrawlerJob>()
                        .WithIdentity(jobKey)
                        .Build();
                    
                    job.JobDataMap["config"] = jobConfig;

                    var trigger = TriggerBuilder.Create()
                        .WithIdentity($"trigger_{jobKey.Name}", "CrawlerTriggers")
                        .WithCronSchedule(jobConfig.Schedule)
                        .Build();

                    await _scheduler.ScheduleJob(job, trigger);

                    _logger.LogInformation("Scheduled job {JobName} with schedule {Schedule}", 
                        jobConfig.CrawlerType, jobConfig.Schedule);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to schedule job {JobName}: {Error}", 
                        jobConfig.CrawlerType, ex.Message);
                }
            }
        }
    }
}