//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using UglyToad.PdfPig;
//using UglyToad.PdfPig.Content;
//using static System.Linq.Enumerable;
//using static System.Linq.Queryable;
//using static UglyToad.PdfPig.Core.PdfSubpath;

//namespace PdfTableReader2
//{
//    public class PlayerResult
//    {
//        public int TeamTotalScore { get; set; }
//        public double TeamAverageScore { get; set; }
//        public int Place { get; set; }
//        public string Rank { get; set; } = string.Empty;
//        public string FullName { get; set; } = string.Empty;
//        public string Region { get; set; } = string.Empty;
//        public int Game1 { get; set; }
//        public int Game2 { get; set; }
//        public int Game3 { get; set; }
//        public int Game4 { get; set; }
//        public int Game5 { get; set; }
//        public int Game6 { get; set; }
//        public int Total { get; set; }
//        public double Average { get; set; }

//        // Информация о команде
//        public bool IsTeam { get; set; }
//        public string TeamId { get; set; } = string.Empty;
//        public string TeamName { get; set; } = string.Empty;

//        // Список игроков в команде (кроме текущего)
//        public List<string> TeamMembers { get; set; } = new List<string>();
//        public List<string> TeamMemberIds { get; set; } = new List<string>();

//        // Дополнительные методы для удобства
//        public int TeamSize { get; set; }

//        public string GetTeamInfo()
//        {
//            if (!IsTeam) return "Индивидуальный участник";

//            var members = string.Join(", ", TeamMembers);
//            return $"Команда: {TeamName}, Место: {Place}, Состав ({TeamSize} чел.): {members}";
//        }
//    }
//    public class LineWordComparer : IEqualityComparer<Word>
//    {
//        public bool Equals(Word a, Word b)
//        {
//            return Math.Abs(a.BoundingBox.Bottom - b.BoundingBox.Bottom) < 1.0;
//        }

//        public int GetHashCode(Word w)
//        {
//            return 0;
//        }
//    }
//    public class PdfTextProcessor
//    {
//        public class TextLine
//        {
//            public string Text { get; private set; } = string.Empty;
//            public List<Word> Words { get; } = new List<Word>();
//            public double MinY => Words.Min(w => w.BoundingBox.Bottom);
//            public double MaxY => Words.Max(w => w.BoundingBox.Top);
//            public double MinX => Words.Min(w => w.BoundingBox.Left);
//            public double MaxX => Words.Max(w => w.BoundingBox.Right);

//            public void AddWord(Word word, string separator = "")
//            {
//                Words.Add(word);
//                Text += separator + word.Text;
//            }
//        }
//        public static IEnumerable<TextLine> GetTextLines(UglyToad.PdfPig.Content.Page page)
//        {
//            const double spaceThreshold = 5.0;
//            var words = page.GetWords().OrderBy(x => x, new PdfWordComparer()).ToList();

//            var lineGroups = words
//                .GroupBy(w => w, new LineWordComparer());

//            foreach (var lineGroup in lineGroups)
//            {
//                var lineWords = lineGroup
//                    .OrderBy(w => w.BoundingBox.Left)
//                    .ToList();

//                var currentLine = new TextLine();

//                foreach (var word in lineWords)
//                {
//                    if (currentLine.Words.Count == 0)
//                    {
//                        currentLine.AddWord(word);
//                        continue;
//                    }

//                    var lastWord = currentLine.Words.Last();
//                    var space = word.BoundingBox.Left - lastWord.BoundingBox.Right;

//                    if (space <= spaceThreshold && space >= 0)
//                    {
//                        currentLine.AddWord(word, " ");
//                    }
//                    else
//                    {
//                        yield return currentLine;
//                        currentLine = new TextLine();
//                        currentLine.AddWord(word);
//                    }
//                }

//                if (currentLine.Words.Any())
//                    yield return currentLine;
//            }
//        }
//    }
//    public class PdfProcessingException : Exception
//    {
//        public PdfProcessingException(string message) : base(message) { }
//        public PdfProcessingException(string message, Exception innerException)
//            : base(message, innerException) { }
//    }

//    public class PdfWordComparer : IComparer<Word>
//    {
//        private readonly double _lineTolerance;
//        public PdfWordComparer(double lineTolerance = 3.0)
//        {
//            _lineTolerance = lineTolerance;
//        }

//        public int Compare(Word x, Word y)
//        {
//            double midY1 = x.BoundingBox.Top + x.BoundingBox.Height / 2;
//            double midY2 = y.BoundingBox.Top + y.BoundingBox.Height / 2;

//            if (Math.Abs(midY1 - midY2) > _lineTolerance)
//            {
//                return midY2.CompareTo(midY1);
//            }

//            return x.BoundingBox.Left.CompareTo(y.BoundingBox.Left);
//        }
//    }
//    public class PdfTableReader
//    {
//        public List<PlayerResult> Results { get; set; } = new List<PlayerResult>();
//        public string? FileName { get; set; } = null;

//        public PdfTableReader() { }
//        public PdfTableReader(string fileName) { FileName = fileName; }

//        public async Task<List<PlayerResult>> GetDataFromPdf(string fileName)
//        {
//            using var document = PdfDocument.Open(fileName);
//            return await GetDataFromPdf(document);
//        }

//        public async Task<List<PlayerResult>> GetDataFromPdf(Stream pdfStream, string fileName)
//        {
//            try
//            {
//                if (!pdfStream.CanRead)
//                    throw new ArgumentException("Stream is not readable", nameof(pdfStream));

//                long originalPosition = pdfStream.CanSeek ? pdfStream.Position : 0;

//                try
//                {
//                    if (pdfStream.CanSeek)
//                        pdfStream.Position = 0;

//                    using (var pdf = PdfDocument.Open(pdfStream))
//                    {
//                        return await GetDataFromPdf(pdf);
//                    }
//                }
//                finally
//                {
//                    if (pdfStream.CanSeek)
//                        pdfStream.Position = originalPosition;
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new PdfProcessingException($"Failed to process PDF '{fileName}'", ex);
//            }
//        }

//        private async Task<List<PlayerResult>> GetDataFromPdf(PdfDocument document)
//        {
//            var page = document.GetPage(1);
//            var textLines = PdfTextProcessor.GetTextLines(page).ToList();
//            var words = textLines.SelectMany(l => l.Words).ToList();

//            // 1. Находим заголовки таблицы
//            var headerWords = DetectHeaderWords(textLines);

//            Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns = new Dictionary<string, (double minX, double maxX, double minY, double maxY)>();
//            foreach (var header in headerWords)
//            {
//                if (!columns.ContainsKey(header.Text))
//                {
//                    if (header.Text.Contains("ФАМИЛИЯ"))
//                    {
//                        var rank = headerWords.FirstOrDefault(o => o.Text == "Звание/");
//                        if (rank != null)
//                        {
//                            columns.Add(header.Text, (minX: rank.MaxX + 5, maxX: header.MaxX + header.MinX - rank.MaxX + 5, minY: header.MinY, maxY: header.MaxY));
//                        }
//                        else
//                        {
//                            columns.Add(header.Text, (minX: header.MinX, maxX: header.MaxX, minY: header.MinY, maxY: header.MaxY));
//                        }
//                    }
//                    else if (header.Text.ToLower() == "Субъект".ToLower())
//                    {
//                        var game1 = headerWords.FirstOrDefault(o => o.Text == "1 игра");
//                        if (game1 != null)
//                        {
//                            columns.Add(header.Text, (minX: header.MinX - (game1.MinX - 5 - header.MaxX), maxX: game1.MinX - 5, minY: header.MinY, maxY: header.MaxY));
//                        }
//                        else
//                        {
//                            columns.Add(header.Text, (minX: header.MinX, maxX: header.MaxX, minY: header.MinY, maxY: header.MaxY));
//                        }
//                    }
//                    else
//                    {
//                        columns.Add(header.Text, (minX: header.MinX, maxX: header.MaxX, minY: header.MinY, maxY: header.MaxY));
//                    }
//                }
//            }

//            // 3. Парсим данные таблицы
//            ParseTableData(words, columns);

//            // 4. После парсинга устанавливаем связи между игроками в командах
//            EstablishTeamRelationships();

//            return Results;
//        }
//        private void EstablishTeamRelationships()
//        {
//            // Группируем по месту (команде)
//            var groups = Results
//                .GroupBy(p => p.Place)
//                .ToList();

//            foreach (var group in groups)
//            {
//                var players = group.ToList();

//                // Определяем, является ли это командой (больше 1 игрока с одинаковым местом)
//                bool isTeam = players.Count > 1;

//                string teamId = isTeam ? $"Team_{group.Key}" : $"Individual_{group.Key}";
//                string teamName = isTeam ? $"Команда #{group.Key}" : "Индивидуальный участник";

//                // Для команд добавляем регион в название
//                if (isTeam)
//                {
//                    var firstPlayer = players.FirstOrDefault(p => !string.IsNullOrEmpty(p.Region));
//                    if (firstPlayer != null)
//                    {
//                        teamName = $"{firstPlayer.Region} (Команда #{group.Key})";
//                    }
//                }

//                foreach (var player in players)
//                {
//                    player.IsTeam = isTeam;
//                    player.TeamId = teamId;
//                    player.TeamName = teamName;

//                    if (isTeam)
//                    {
//                        // Устанавливаем связи между игроками команды
//                        player.TeamMembers = players
//                            .Where(p => p != player)
//                            .Select(p => p.FullName)
//                            .ToList();

//                        player.TeamMemberIds = players
//                            .Where(p => p != player)
//                            .Select(p => GetPlayerId(p))
//                            .ToList();
//                    }
//                }
//            }
//        }
//        private List<List<Word>> GroupWordsByLines(List<Word> words)
//        {
//            // Группируем слова по строкам с учетом Y-координат
//            var orderedWords = words.OrderBy(x => x, new PdfWordComparer()).ToList();

//            var lines = new List<List<Word>>();
//            List<Word> currentLine = null;
//            double? lastY = null;

//            foreach (var word in orderedWords)
//            {
//                double midY = word.BoundingBox.Top + word.BoundingBox.Height / 2;

//                if (currentLine == null || (lastY.HasValue && Math.Abs(midY - lastY.Value) > 5))
//                {
//                    // Новая строка
//                    if (currentLine != null)
//                    {
//                        lines.Add(currentLine);
//                    }
//                    currentLine = new List<Word> { word };
//                }
//                else
//                {
//                    // Текущая строка
//                    currentLine.Add(word);
//                }

//                lastY = midY;
//            }

//            if (currentLine != null && currentLine.Any())
//            {
//                lines.Add(currentLine);
//            }

//            return lines;
//        }
//        private List<PdfTextProcessor.TextLine> DetectHeaderWords(List<PdfTextProcessor.TextLine> textLines)
//        {
//            var headerTitles = new[] { "место", "Звание/", "ФАМИЛИЯ", "Субъект", "1 игра", "2 игра", "3 игра", "4 игра", "5 игра", "6 игра", "сумма 6", "средний" };

//            return textLines
//                .Where(w => headerTitles.Any(t => w.Text.StartsWith(t, StringComparison.OrdinalIgnoreCase)))
//                .ToList();
//        }
//        private void ParseTableData(List<Word> words, Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns)
//        {
//            // Находим Y-координату начала данных
//            var headerBottom = columns.Values.Max(c => c.maxY);
//            var dataWords = words.Where(w => w.BoundingBox.Top < headerBottom - 20).ToList();

//            // Группируем слова по строкам
//            var lines = GroupWordsByLines(dataWords);

//            // Находим строки с местами и их диапазоны
//            var placeRanges = new List<(int Place, double MinY, double MaxY, double CenterY, List<Word> Line)>();

//            foreach (var line in lines)
//            {
//                var placeWord = line.FirstOrDefault(w =>
//                    w.BoundingBox.Left >= columns["место"].minX &&
//                    w.BoundingBox.Left <= columns["место"].maxX &&
//                    int.TryParse(w.Text, out _));

//                if (placeWord != null && int.TryParse(placeWord.Text, out int place))
//                {
//                    double minY = line.Min(w => w.BoundingBox.Top);
//                    double maxY = line.Max(w => w.BoundingBox.Bottom);
//                    double centerY = (minY + maxY) / 2;

//                    // Расширяем диапазон для объединенных ячеек
//                    // Объединенная ячейка обычно занимает высоту нескольких строк
//                    double height = maxY - minY;
//                    double expandedMinY = centerY - height * 2; // Предполагаем, что команда может быть до 4 строк
//                    double expandedMaxY = centerY + height * 2;

//                    placeRanges.Add((place, expandedMinY, expandedMaxY, centerY, line));
//                }
//            }

//            // Если нашли места, распределяем строки по командам
//            if (placeRanges.Any())
//            {
//                // Сортируем по Y для удобства
//                placeRanges = placeRanges.OrderBy(p => p.CenterY).ToList();
//                lines = lines.OrderBy(l => l.Min(w => w.BoundingBox.Top)).ToList();

//                // Вычисляем среднее расстояние между центрами
//                double avgDistance = 0;
//                if (placeRanges.Count > 1)
//                {
//                    double totalDistance = 0;
//                    for (int i = 1; i < placeRanges.Count; i++)
//                    {
//                        totalDistance += Math.Abs(placeRanges[i].CenterY - placeRanges[i - 1].CenterY);
//                    }
//                    avgDistance = totalDistance / (placeRanges.Count - 1);
//                }
//                else
//                {
//                    // Если только одно место, берем высоту строки как ориентир
//                    avgDistance = placeRanges.First().MaxY - placeRanges.First().MinY;
//                }

//                // Распределяем строки по диапазонам
//                var unassignedLines = new List<List<Word>>();
//                var teamAssignments = new Dictionary<int, List<List<Word>>>();

//                // Инициализируем команды
//                foreach (var placeRange in placeRanges)
//                {
//                    teamAssignments[placeRange.Place] = new List<List<Word>> { placeRange.Line };
//                }

//                // Распределяем оставшиеся строки
//                foreach (var line in lines)
//                {
//                    // Пропускаем строки, которые уже в командах (те, что с местами)
//                    if (placeRanges.Any(p => p.Line == line))
//                        continue;

//                    double lineCenterY = line.Average(w => w.BoundingBox.Top);

//                    // Ищем подходящую команду
//                    var suitableTeams = placeRanges
//                        .Where(p => lineCenterY >= p.MinY && lineCenterY <= p.MaxY)
//                        .ToList();

//                    if (suitableTeams.Count == 1)
//                    {
//                        // Однозначно определяем команду
//                        teamAssignments[suitableTeams[0].Place].Add(line);
//                    }
//                    else if (suitableTeams.Count > 1)
//                    {
//                        // Если несколько подходящих команд, выбираем ближайшую по центру
//                        var nearestTeam = suitableTeams
//                            .OrderBy(p => Math.Abs(lineCenterY - p.CenterY))
//                            .First();
//                        teamAssignments[nearestTeam.Place].Add(line);
//                    }
//                    else
//                    {
//                        // Не нашли подходящую команду - сохраняем для последующей обработки
//                        unassignedLines.Add(line);
//                    }
//                }

//                // Обрабатываем нераспределенные строки (возможно, индивидуальные игроки)
//                int nextPlace = teamAssignments.Keys.Any() ? teamAssignments.Keys.Max() + 1 : 1;
//                foreach (var line in unassignedLines)
//                {
//                    teamAssignments[nextPlace] = new List<List<Word>> { line };
//                    nextPlace++;
//                }

//                // Сохраняем все команды
//                foreach (var team in teamAssignments)
//                {
//                    SaveTeam(team.Value, team.Key, columns);
//                }
//            }
//            else
//            {
//                // Не нашли мест - сохраняем все как отдельные строки
//                for (int i = 0; i < lines.Count; i++)
//                {
//                    SaveTeam(new List<List<Word>> { lines[i] }, i + 1, columns);
//                }
//            }
//        }

//        private int FindNextAvailablePlace(Dictionary<int, List<List<Word>>> teamAssignments)
//        {
//            if (!teamAssignments.Any())
//                return 1;

//            int maxPlace = teamAssignments.Keys.Max();

//            // Ищем первое свободное место
//            for (int i = 1; i <= maxPlace + 1; i++)
//            {
//                if (!teamAssignments.ContainsKey(i))
//                    return i;
//            }

//            return maxPlace + 1;
//        }

//        private void SaveTeam(List<List<Word>> teamLines, int teamPlace, Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns)
//        {
//            if (!teamLines.Any())
//                return;

//            bool isTeam = teamLines.Count > 1;

//            // Парсим каждого игрока в команде
//            foreach (var line in teamLines)
//            {
//                var player = ParseLineToPlayerResult(line, columns);
//                player.Place = teamPlace;
//                player.IsTeam = isTeam;

//                // Добавляем в результаты
//                Results.Add(player);
//            }

//            // После сохранения всех игроков устанавливаем связи между ними
//            if (isTeam)
//            {
//                var teamPlayers = Results.Where(p => p.Place == teamPlace && p.IsTeam).ToList();
//                EstablishTeamRelationshipsForTeam(teamPlayers, teamPlace);
//            }
//        }

//        private void EstablishTeamRelationshipsForTeam(List<PlayerResult> teamPlayers, int teamPlace)
//        {
//            if (teamPlayers.Count < 2)
//                return;

//            string teamId = $"Team_{teamPlace}";

//            // Определяем название команды (берем из первого игрока с регионом)
//            var firstPlayerWithRegion = teamPlayers.FirstOrDefault(p => !string.IsNullOrEmpty(p.Region));
//            string teamName = firstPlayerWithRegion != null && !string.IsNullOrEmpty(firstPlayerWithRegion.Region)
//                ? $"{firstPlayerWithRegion.Region} (Команда #{teamPlace})"
//                : $"Команда #{teamPlace}";

//            // Устанавливаем связи для всех игроков команды
//            foreach (var player in teamPlayers)
//            {
//                player.TeamId = teamId;
//                player.TeamName = teamName;

//                // Список товарищей по команде
//                player.TeamMembers = teamPlayers
//                    .Where(p => p != player)
//                    .Select(p => p.FullName)
//                    .ToList();

//                player.TeamMemberIds = teamPlayers
//                    .Where(p => p != player)
//                    .Select(p => GetPlayerId(p))
//                    .ToList();

//                // Дополнительная информация о команде
//                player.TeamSize = teamPlayers.Count;
//                player.TeamTotalScore = teamPlayers.Sum(p => p.Total);
//                player.TeamAverageScore = teamPlayers.Average(p => p.Average);
//            }
//        }


//        private string GetPlayerId(PlayerResult player)
//        {
//            // Создаем уникальный идентификатор игрока
//            return $"{player.FullName}_{player.Region}_{player.Place}".GetHashCode().ToString();
//        }

//        private PlayerResult ParseLineToPlayerResult(List<Word> line, Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns)
//        {
//            var result = new PlayerResult();

//            // Сортируем слова по X для правильного парсинга
//            var sortedWords = line.OrderBy(w => w.BoundingBox.Left).ToList();

//            foreach (var word in sortedWords)
//            {
//                // Ищем соответствующую колонку
//                foreach (var column in columns)
//                {
//                    if (word.BoundingBox.Left >= column.Value.minX - 2 &&  // Небольшой допуск
//                        word.BoundingBox.Left <= column.Value.maxX + 2)
//                    {
//                        UpdateColumn(result, column.Key, word.Text);
//                        break;
//                    }
//                }
//            }

//            return result;
//        }

//        private void UpdateColumn(PlayerResult row, string columnName, string value)
//        {
//            // Убираем дублирование из названий столбцов
//            string[] switchStrings = { "место", "Звание/", "ФАМИЛИЯ", "Субъект", "1 игра", "2 игра", "3 игра", "4 игра", "5 игра", "6 игра", "сумма 6", "средний" };

//            var match = switchStrings.FirstOrDefault(s => columnName.Contains(s));

//            switch (match)
//            {
//                case "место":
//                    // Место уже устанавливается в SaveTeam
//                    break;
//                case "Звание/":
//                    row.Rank = (row.Rank + " " + value).Trim();
//                    break;
//                case "ФАМИЛИЯ":
//                    row.FullName = (row.FullName + " " + value).Trim();
//                    break;
//                case "Субъект":
//                    row.Region = (row.Region + " " + value).Trim();
//                    break;
//                case "1 игра":
//                    if (int.TryParse(value, out int game1)) row.Game1 = game1;
//                    break;
//                case "2 игра":
//                    if (int.TryParse(value, out int game2)) row.Game2 = game2;
//                    break;
//                case "3 игра":
//                    if (int.TryParse(value, out int game3)) row.Game3 = game3;
//                    break;
//                case "4 игра":
//                    if (int.TryParse(value, out int game4)) row.Game4 = game4;
//                    break;
//                case "5 игра":
//                    if (int.TryParse(value, out int game5)) row.Game5 = game5;
//                    break;
//                case "6 игра":
//                    if (int.TryParse(value, out int game6)) row.Game6 = game6;
//                    break;
//                case "сумма 6":
//                    if (int.TryParse(value, out int total)) row.Total = total;
//                    break;
//                case "средний":
//                    if (double.TryParse(value.Replace(',', '.'), out double avg)) row.Average = avg;
//                    break;
//            }
//        }

//    }
//}

