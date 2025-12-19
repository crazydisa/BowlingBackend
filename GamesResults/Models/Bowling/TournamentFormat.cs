using System;
using System.Collections.Generic;
using System.Text;

namespace GamesResults.Models.Bowling
{
    public enum TournamentFormat
    {
        Singles = 0,          // Одиночный разряд
        Doubles = 1,          // Парный разряд (2 человека)
        Triples = 2,          // Тройки (3 человека)
        TeamOfFour = 3,       // Команда из 4 человек
        TeamOfFive = 4,       // Команда из 5 человек
        Baker = 5,           // Формат Бейкера (последовательные фреймы)
        MatchPlay = 6,       // Матчевая встреча
        Unknown = 7
    }
}
