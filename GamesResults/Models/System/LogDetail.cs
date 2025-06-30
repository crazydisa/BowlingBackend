namespace GamesResults.Models
{
    public class LogDetail
    {
        public Guid Id { get; set; }
        public Guid LogId { get; set; }
        public Log? Log { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}
