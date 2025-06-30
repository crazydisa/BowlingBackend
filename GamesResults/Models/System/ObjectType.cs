using GamesResults.Interfaces;

namespace GamesResults.Models
{
    /// <summary>
    /// Тип объекта
    /// </summary>
    public class ObjectType : INamed, ITitled
    {
        public ObjectType()
        {
            Properties = new List<ObjectProperty>();
        }

        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? NameSpace { get; set; } = null!;
        public string Title { get; set; } = null!;

        public string? DisplayExpr { get; set; } = null!;

        public long? RootContainerId { get; set; }
        public Container? RootContainer { get; set; }

        public List<ObjectProperty> Properties { get; set; }
    }
}
