namespace GamesResults.Models
{
    /// <summary>
    /// Страница пользовательского интерфейса приложения
    /// </summary>
    public class One2Many2
    {
        public string OneTypeName { get; set; }
        public string? OneTypeNameSpace { get; set; }
        public string ManyTypeName { get; set; }
        public string? ManyTypeNameSpace { get; set; }

        public object OneObject { get; set; }
        public object[] ManyObject { get; set; }
        public RequestOption RequestOption { get; set; } = new RequestOption() { typeName = "One2Many2", nameSpace = "PirAppBp.Models.Sapsan", actionName = "remove" };
    }
}
