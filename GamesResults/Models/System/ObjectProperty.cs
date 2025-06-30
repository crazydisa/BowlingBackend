using GamesResults.Interfaces;
using System.Text.Json.Serialization;

namespace GamesResults.Models
{
    /// <summary>
    /// Тип объекта
    /// </summary>
    public class ObjectProperty : INamed, ITitled
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? GroupTitle { get; set; }

        public string? SubGroupTitle { get; set; }

        public string? Description { get; set; }

        public int? SortIndex { get; set; }

        [JsonIgnore]
        public int? ObjectTypeId { get; set; }

        [JsonIgnore]
        public ObjectType? ObjectType { get; set; }

        public string? TypeName { get; set; }
        public string? NameSpace { get; set; }

        public string? DictionaryTypeName { get; set; }

        public string? DataFormat { get; set; }

        public string? DisplayExpr { get; set; }

        public string? RelatedField { get; set; }

        public string? RelatedType { get; set; }

        public bool IsIdentifier { get; set; }

        public bool IsNumeric { get; set; }

        public bool IsBoolean { get; set; }

        public bool IsString { get; set; }

        public bool IsMultiline { get; set; }

        public bool IsGuid { get; set; }

        public bool IsDate { get; set; }

        public bool IsObject { get; set; }

        public bool IsInclude { get; set; }

        public bool IsArray { get; set; }

        public bool IsNullable { get; set; }

        public bool IsHiddenByDefault { get; set; }

        public bool IsHiddenInLogDetail { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsNotAvailable { get; set; }


        public List<Role>? EditRoles { get; set; }
    }
}
