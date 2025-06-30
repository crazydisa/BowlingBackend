namespace GamesResults.Models
{
    /// <summary>
    /// Страница пользовательского интерфейса приложения
    /// </summary>
    public class Many2Many
    {
        public string FirstTypeName { get; set; }
        public string SecondTypeName { get; set; }
        public string LinkTypeName { get; set; }

        public object FirstObject { get; set; }
        public object SecondObject { get; set; }
    }
}
