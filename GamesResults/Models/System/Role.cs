using GamesResults.Interfaces;
using System.Text.Json.Serialization;

namespace GamesResults.Models
{
    /// <summary>
    /// Роль пользователя в системе, для которой определены разрешенные действия и доступы к Модулям, Страницам, Компонентам и Объектам 
    /// </summary>
    public class Role : INamed, ITitled
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Title { get; set; } = null!;

        public bool IsAdmin { get; set; }

        public bool IsPosition { get; set; }

        public bool IsDepartment { get; set; }

        [JsonIgnore]
        public List<User>? Users { get; set; }

        [JsonIgnore]
        public List<Action>? Actions { get; set; }

        [JsonIgnore]
        public List<ObjectProperty>? EditProperties { get; set; }
    }
}