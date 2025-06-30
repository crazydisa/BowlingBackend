using GamesResults.Interfaces;

namespace GamesResults.Models
{
    /// <summary>
    /// Действие - создание, получение, изменение, удаление объектов, а также другие специфичные действия
    /// </summary>
    public class Action : INamed, ITitled
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Title { get; set; } = null!;

        public int? ObjectTypeId { get; set; }
        public ObjectType? ObjectType { get; set; }

        public bool IsDefault { get; set; }

        public bool IsChange { get; set; }

        public bool IsLogDetails { get; set; }

        public List<Role>? Roles { get; set; }
    }
}
