namespace Librarian.Scheduler.Models
{
    public class ScheduledJobModel
    {
        public bool IsEnabled { get; set; } = true;
        public string Schedule { get; set; } = string.Empty;
        public string ScheduleDisplay { get; set; } = string.Empty;
        public string CrawlerAssembly { get; set; } = string.Empty;
        public string CrawlerType { get; set; } = string.Empty;
        public string CrawlerTypeDisplay { get; set; } = string.Empty;
        public string Parameter { get; set; } = string.Empty;
        public int Id { get; set; }
    }

    public class CrawlerInfo
    {
        public string AssemblyName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class ScheduleOption
    {
        public string Display { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
    }
}