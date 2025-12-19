using System;
using System.Collections.Generic;
using System.Text;

namespace GamesResults.Models.Bowling
{
    // Система подсчета очков
    public enum ScoringSystem
    {
        Scratch = 0,          // По фактическим очкам
        Handicap = 1,         // С гандикапом
        PointsBased = 2,      // Система очков (победа = 2 очка, ничья = 1)
        Elimination = 3,      // На выбывание
        RoundRobin = 4,       // Круговой турнир
        Swiss = 5            // Швейцарская система
    }
}
