namespace GamesResults.Models
{
    /// <summary>
    /// Компонент пользовательского интерфейса приложения
    /// </summary>
    public class Component : Container
    {
        public int? LoadActionId { get; set; }
        public Action? LoadAction { get; set; }
    }
}
