using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    // Программа масла
    public class Oil: DictionaryItem
    {
        // Схема нанесения
        public string? Pattern { get; set; } 

        // Турниры на этом масле
        public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    }
}
