namespace GamesResults.Models
{
    /// <summary>
    /// Страница пользовательского интерфейса приложения
    /// </summary>
    public class RequestOption
    {
        public string typeName { get; set; }
        public string nameSpace { get; set; }
        public string? condition { get; set; }
        public string? actionName { get; set; }
        public string? getterName { get; set; }
        public string? setterName { get; set; }
        public string? url { get; set; }
        public string? accesName { get; set; }
        public bool? useFilterIds { get; set; } = false;
        public string? idPropName { get; set; }
        public string? idPropTypeName { get; set; }
        public object[]? ids { get; set; }

    }
}
