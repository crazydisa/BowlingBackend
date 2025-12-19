using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Tournament: Object
    {
        [displayExpr]
        public string Name { get; set; } = string.Empty;
        // Тип и формат
        public TournamentType TournamentType { get; set; } = TournamentType.Unknown;
        public TournamentFormat Format { get; set; } = TournamentFormat.Unknown;
        public ScoringSystem ScoringSystem { get; set; } = ScoringSystem.Scratch;

        // Даты проведения
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Правила формирования команд
        public int? MaxTeamSize { get; set; }
        public int? MinTeamSize { get; set; }

        // Статус обработки рейтингов
        public bool RatingsUpdated { get; set; }
        public DateTime? RatingsUpdatedDate { get; set; }

        // Связи (Боулинг центр)
        public long? BowlingId { get; set; }
        public virtual Bowling? Bowling { get; set; } = null!;

        //Программа масла
        [ObjectPropertyAttribute(TypeName = "Oil", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? OilId { get; set; }
        public virtual Oil? Oil { get; set; }
        [ObjectPropertyAttribute(TypeName = "City", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? CityId { get; set; }
        public City? City { get; set; }
        
        // Коллекция документов
        public virtual ICollection<TournamentDocument> Documents { get; set; } = new List<TournamentDocument>();

        // Коллекция участников
        public virtual ICollection<BaseTournamentResult> Results { get; set; } = new List<BaseTournamentResult>();
        // Навигационное свойство для истории изменений
        public virtual ICollection<RatingHistory> History { get; set; } = new List<RatingHistory>();
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

        // Вычисляемые свойства
        [NotMapped]
        public bool HasTeams => TournamentType == TournamentType.Team ||
                               TournamentType == TournamentType.Mixed;

        [NotMapped]
        public bool HasIndividualResults => TournamentType == TournamentType.Individual ||
                                          TournamentType == TournamentType.Mixed;

        [NotMapped]
        public int? DurationDays
        {
            get
            {
                if (!StartDate.HasValue || !EndDate.HasValue)
                    return null;

                if (EndDate.Value < StartDate.Value)
                    return 0;

                return (EndDate.Value - StartDate.Value).Days + 1;
            }
        }
    }
   
}
