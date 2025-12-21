using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    // Боулинг-центр
    public class Bowling: Object
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }

        // Связь с городом
        public long CityId { get; set; }
        public virtual City City { get; set; } = null!;

        // Турниры в этом центре
        public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    }
}
