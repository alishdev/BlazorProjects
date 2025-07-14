namespace Librarian.Service.Models
{
    public class ScheduledJobConfig
    {
        public bool IsEnabled { get; set; }
        public string Schedule { get; set; } = string.Empty;
        public string CrawlerAssembly { get; set; } = string.Empty;
        public string CrawlerType { get; set; } = string.Empty;
        public object? Parameter { get; set; }
    }
}