namespace GamesResults.Models
{
    /// <summary>
    /// Страница пользовательского интерфейса приложения
    /// </summary>
    public class One2Many
    {
        public string OneTypeName { get; set; }
        public string ManyTypeName { get; set; }

        public object OneObject { get; set; }
        public object ManyObject { get; set; }
    }
}
