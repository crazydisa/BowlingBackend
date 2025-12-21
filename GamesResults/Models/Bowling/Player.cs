using GamesResults.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Player : DictionaryItem
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Country { get; set; }

        // Добавляем свойство пола
        public Gender Gender { get; set; } = Gender.Unknown;

        [ObjectPropertyAttribute(TypeName = "City", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? CityId { get; set; }
        public City? City { get; set; }

        [ObjectPropertyAttribute(TypeName = "District", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? DistrictId { get; set; } = null;
        public virtual District? District { get; set; }

        // Рейтинг игрока (отношение 1:1)
        [ObjectPropertyAttribute(TypeName = "PlayerRating", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long PlayerRatingId { get; set; }
        public virtual PlayerRating? PlayerRating { get; set; } // Навигационное свойство

        // Участие в командах
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

        // Индивидуальные результаты
        public virtual ICollection<IndividualResult> IndividualResults { get; set; } = new List<IndividualResult>();

        // Вычисляемые свойства (не сохраняются в БД)
        [NotMapped]
        public int? Age => BirthDate.HasValue ?
            DateTime.Now.Year - BirthDate.Value.Year : null;
        [NotMapped]
        public string? Region => District?.Name ?? City?.Name;
        [NotMapped]
        public bool IsMale => Gender == Gender.Male;
        [NotMapped]
        public bool IsFemale => Gender == Gender.Female;
        [NotMapped]
        public string GenderDisplay => Gender switch
        {
            Gender.Male => "Мужской",
            Gender.Female => "Женский",
            _ => "Не указан"
        };

        // Метод для автоматического определения пола по ФИО
        public void DetermineGenderFromName()
        {
            if (!string.IsNullOrWhiteSpace(this.Name))
            {
                this.Gender = GenderDetector.DetectGender(this.Name);
            }
        }
    }
}
