using GamesResults.Interfaces;

namespace GamesResults.Models
{
    /// <summary>
    /// Контейнер объектов
    /// </summary>
    public class Container : Object, INamed
    {
        public string Name { get; set; } = null!;
    }
}
