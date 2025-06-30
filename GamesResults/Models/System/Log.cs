namespace GamesResults.Models
{
    public class Log
    {
        public Guid Id { get; set; }
        public int ActionId { get; set; }
        public Action? Action { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public long? ObjectId { get; set; }
        public Object? Object { get; set; }
        public DateTime LogTime { get; set; }
        public int LogLevel { get; set; }
        public string? LogText { get; set; }
    }
}
