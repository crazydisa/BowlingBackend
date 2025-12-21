using GamesResults.Interfaces;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GamesResults.Models
{
    /// <summary>
    /// Базовый объект
    /// </summary>
    public abstract class Object : ITitled, IDeleted, ICreated, IEditable
    {

        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; } 

        public int? TypeId { get; set; }

        [JsonIgnore]
        public ObjectType? Type { get; set; }

        public long? ParentId { get; set; }

        [JsonIgnore]
        public Object? Parent { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? SortIndex { get; set; }

        public string? IconName { get; set; }

        [JsonIgnore]
        public bool IsSystem { get; set; }

        public DateTime? CreatedAt { get; set; }
        [Models.ObjectPropertyAttribute(TypeName = "User", DisplayExpr = "Title", NameSpace = "GamesResults.Models")]
        public long? AuthorId { get; set; }
        [JsonIgnore]
        public User? Author { get; set; }


        public DateTime? ModifiedAt { get; set; }

        public long? EditorId { get; set; }
        [JsonIgnore]
        public User? Editor { get; set; }

        [JsonIgnore]
        public int? Version { get; set; }

        //[JsonIgnore]
        public DateTime? DeletedAt { get; set; }

        [JsonIgnore]
        public long? DeleterId { get; set; }

        [JsonIgnore]
        public User? Deleter { get; set; }

        [JsonIgnore]
        public bool? IsDeleted { get; set; }
    }
}
