using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using static PdfTableReader2.GenderDetector;
//using static UglyToad.PdfPig.Core.Union<A, B>;

namespace PdfTableReader2
{
    public class PdfProcessingException : Exception
    {
        public PdfProcessingException(string message) : base(message) { }
        public PdfProcessingException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class PdfTableReader
    {
        public List<List<PlayerResult>> AllResults { get; set; } = new List<List<PlayerResult>>();
        public List<PlayerResult> Results => AllResults.FirstOrDefault() ?? new List<PlayerResult>();
        public string? FileName { get; set; } = null;

        public PdfTableReader() { }
        public PdfTableReader(string fileName) { FileName = fileName; }

        public async Task<List<List<PlayerResult>>> GetAllTablesFromPdf(string fileName)
        {
            using var document = PdfDocument.Open(fileName);
            return await GetAllTablesFromPdf(document);
        }

        public async Task<List<List<PlayerResult>>> GetAllTablesFromPdf(Stream pdfStream, string fileName)
        {
            try
            {
                if (!pdfStream.CanRead)
                    throw new ArgumentException("Stream is not readable", nameof(pdfStream));

                long originalPosition = pdfStream.CanSeek ? pdfStream.Position : 0;

                try
                {
                    if (pdfStream.CanSeek)
                        pdfStream.Position = 0;

                    using (var pdf = PdfDocument.Open(pdfStream))
                    {
                        return await GetAllTablesFromPdf(pdf);
                    }
                }
                finally
                {
                    if (pdfStream.CanSeek)
                        pdfStream.Position = originalPosition;
                }
            }
            catch (Exception ex)
            {
                throw new PdfProcessingException($"Failed to process PDF '{fileName}'", ex);
            }
        }

        private async Task<List<List<PlayerResult>>> GetAllTablesFromPdf(PdfDocument document)
        {
            var page = document.GetPage(1);
            var textLines = PdfTextProcessor.GetTextLines(page).ToList();
            var words = textLines.SelectMany(l => l.Words).ToList();

            // 1. Находим ВСЕ заголовки таблиц на странице
            var allHeaders = DetectAllTableHeaders(textLines);

            // 2. Для каждой найденной таблицы парсим данные
            foreach (var headers in allHeaders)
            {
                var tableResults = ParseTableFromHeaders(words, headers);
                if (tableResults.Any())
                {
                    AllResults.Add(tableResults);
                }
            }
            AllResults = AllResults.Select(table => FilterValidResults(table)).ToList();
            return AllResults;
        }
        private List<List<PdfTextProcessor.TextLine>> DetectAllTableHeaders(List<PdfTextProcessor.TextLine> textLines)
        {
            var allTables = new List<List<PdfTextProcessor.TextLine>>();

            // 1. Находим все строки, которые могут быть заголовками таблиц
            var headerCandidates = FindHeaderCandidates(textLines);

            if (!headerCandidates.Any())
                return allTables;

            // 2. Группируем кандидатов в кластеры по вертикали
            var clusters = ClusterByVerticalPosition(headerCandidates, tolerance: 15.0);

            // 3. Фильтруем кластеры, оставляя только те, что похожи на заголовки таблиц
            foreach (var cluster in clusters)
            {
                if (IsTableHeaderCluster(cluster))
                {
                    // 4. Для каждого кластера находим все строки, относящиеся к заголовкам
                    var tableHeaders = BuildCompleteTableHeaders(cluster, textLines);
                    if (tableHeaders.Any())
                    {
                        allTables.Add(tableHeaders);
                    }
                }
            }

            // 5. Сортируем таблицы сверху вниз
            allTables = allTables
                .OrderByDescending(table => table.Max(line => line.MaxY))
                .ToList();

            return allTables;
        }

        private List<PdfTextProcessor.TextLine> FindHeaderCandidates(List<PdfTextProcessor.TextLine> textLines)
        {
            var candidates = new List<PdfTextProcessor.TextLine>();

            // Ключевые слова для заголовков таблиц
            var keyPhrases = new[]
            {
                "место",
                "Звание/",
                "ФАМИЛИЯ",
                "Субъект",
                "игра"
            };

            foreach (var line in textLines)
            {
                string lineText = line.Text;

                // Проверяем несколько условий
                bool isCandidate = false;

                // Условие 1: содержит ключевые фразы
                isCandidate |= keyPhrases.Any(phrase =>
                    lineText.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) >= 0);

                // Условие 2: короткий текст (заголовки обычно короткие)
                //isCandidate |= lineText.Length < 50 && lineText.Split(' ').Length <= 3;

                // Условие 3: содержит числа (для заголовков игр: "1 игра", "2 игра" и т.д.)
                isCandidate |= System.Text.RegularExpressions.Regex.IsMatch(lineText, @"\d+\s+игра", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (isCandidate)
                {
                    candidates.Add(line);
                }
            }

            return candidates;
        }
        private List<List<PdfTextProcessor.TextLine>> ClusterByVerticalPosition(List<PdfTextProcessor.TextLine> candidates, double tolerance)
        {
            if (!candidates.Any())
                return new List<List<PdfTextProcessor.TextLine>>();

            var sortedCandidates = candidates.OrderByDescending(c => c.MaxY).ToList();
            var clusters = new List<List<PdfTextProcessor.TextLine>>();

            var currentCluster = new List<PdfTextProcessor.TextLine> { sortedCandidates[0] };
            double clusterReferenceY = sortedCandidates[0].MaxY;

            for (int i = 1; i < sortedCandidates.Count; i++)
            {
                var candidate = sortedCandidates[i];
                double candidateY = candidate.MaxY;

                if (Math.Abs(candidateY - clusterReferenceY) <= tolerance)
                {
                    // Входит в текущий кластер
                    currentCluster.Add(candidate);
                    // Обновляем reference Y как среднее
                    clusterReferenceY = currentCluster.Average(c => c.MaxY);
                }
                else
                {
                    // Начинаем новый кластер
                    clusters.Add(new List<PdfTextProcessor.TextLine>(currentCluster));
                    currentCluster = new List<PdfTextProcessor.TextLine> { candidate };
                    clusterReferenceY = candidate.MaxY;
                }
            }

            if (currentCluster.Any())
            {
                clusters.Add(currentCluster);
            }

            return clusters;
        }
        private bool IsTableHeaderCluster(List<PdfTextProcessor.TextLine> cluster)
        {
            if (cluster.Count < 3)
                return false;

            // Объединяем весь текст кластера
            var allText = string.Join(" ", cluster.Select(l => l.Text));

            // Проверяем наличие ключевых элементов
            bool hasPlace = allText.Contains("место", StringComparison.OrdinalIgnoreCase);
            bool hasName = allText.Contains("фамилия", StringComparison.OrdinalIgnoreCase) ||
                           allText.Contains("ФАМИЛИЯ", StringComparison.OrdinalIgnoreCase);
            bool hasGame = allText.Contains("игра", StringComparison.OrdinalIgnoreCase);
            bool hasRank = allText.Contains("звание", StringComparison.OrdinalIgnoreCase);
            bool hasRegion = allText.Contains("субъект", StringComparison.OrdinalIgnoreCase);

            // Для таблицы результатов должно быть минимум 3 из 5 ключевых элементов
            int keyElementsCount = 0;
            if (hasPlace) keyElementsCount++;
            if (hasName) keyElementsCount++;
            if (hasGame) keyElementsCount++;
            if (hasRank) keyElementsCount++;
            if (hasRegion) keyElementsCount++;

            return keyElementsCount >= 3;
        }
        private List<PdfTextProcessor.TextLine> BuildCompleteTableHeaders(List<PdfTextProcessor.TextLine> cluster, List<PdfTextProcessor.TextLine> allLines)
        {
            // Находим границы кластера
            double minY = cluster.Min(l => l.MinY);
            double maxY = cluster.Max(l => l.MaxY);
            double avgY = cluster.Average(l => (l.MinY + l.MaxY) / 2);

            // Расширяем область поиска выше и ниже кластера
            double searchMinY = minY - 10; // 30 единиц выше
            double searchMaxY = maxY + 10; // 30 единиц ниже

            // Ищем все строки в расширенной области, которые могут быть частью заголовков
            var potentialHeaders = allLines
                .Where(line =>
                    line.MinY >= searchMinY &&
                    line.MaxY <= searchMaxY &&
                    !string.IsNullOrWhiteSpace(line.Text))
                .OrderByDescending(line => line.MaxY)
                .ToList();

            // Фильтруем строки, которые логически связаны с заголовками таблицы
            var tableHeaders = new List<PdfTextProcessor.TextLine>();

            foreach (var line in potentialHeaders)
            {
                string lineText = line.Text;

                // Критерии для включения строки в заголовки таблицы:
                bool shouldInclude = false;

                // 1. Уже есть в исходном кластере
                shouldInclude |= cluster.Contains(line);

                // 2. Содержит ключевые слова заголовков
                shouldInclude |= lineText.Contains("место", StringComparison.OrdinalIgnoreCase);
                shouldInclude |= lineText.Contains("звание", StringComparison.OrdinalIgnoreCase);
                shouldInclude |= lineText.Contains("фамилия", StringComparison.OrdinalIgnoreCase);
                shouldInclude |= lineText.Contains("субъект", StringComparison.OrdinalIgnoreCase);
                shouldInclude |= lineText.Contains("игра", StringComparison.OrdinalIgnoreCase);
                shouldInclude |= lineText.Contains("сумма", StringComparison.OrdinalIgnoreCase);
                shouldInclude |= lineText.Contains("средний", StringComparison.OrdinalIgnoreCase);

                // 3. Содержит номер игры (1, 2, 3 и т.д.)
                shouldInclude |= System.Text.RegularExpressions.Regex.IsMatch(lineText, @"^\d+\s*игра", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // 4. Очень короткая строка (возможно, часть заголовка)
                shouldInclude |= lineText.Length <= 15 && lineText.Split(' ').Length <= 2;

                if (shouldInclude)
                {
                    tableHeaders.Add(line);
                }
            }

            // Убираем дубликаты и сортируем по вертикали
            tableHeaders = tableHeaders
                .Distinct()
                .OrderByDescending(l => l.MaxY)
                .ToList();

            return tableHeaders;
        }
        private List<PlayerResult> ParseTableFromHeaders(List<Word> allWords, List<PdfTextProcessor.TextLine> tableHeaders)
        {
            var results = new List<PlayerResult>();

            // Определяем колонки для этой таблицы
            var columns = DetectColumnsFromHeaders(tableHeaders);

            if (!columns.Any())
                return results;

            // Находим Y-координату НИЖНЕЙ границы заголовка (самый нижний Y среди заголовков)
            var headerBottom = columns.Values.Min(c => c.minY); // minY - это нижняя граница

            // Находим Y-координату ВЕРХНЕЙ границы заголовка (самый верхний Y среди заголовков)
            var headerTop = columns.Values.Max(c => c.maxY); // maxY - это верхняя граница

            Console.WriteLine($"Заголовок таблицы: Top={headerTop:F1} (верх), Bottom={headerBottom:F1} (низ)");

            // Ищем следующую таблицу
            double? nextTableTop = null;
            var textLinesFromWords = PdfTextProcessor.GetTextLinesFromWords(allWords);
            var allHeaders = DetectAllTableHeaders(textLinesFromWords);

            // Сортируем таблицы по Y (сверху вниз) - чем больше Y, тем выше на странице
            var sortedTables = allHeaders
                .Select((headers, index) => new
                {
                    Headers = headers,
                    Index = index,
                    TableTop = headers.Max(h => h.MaxY), // Верхняя граница таблицы
                    TableBottom = headers.Min(h => h.MinY) // Нижняя граница таблицы
                })
                .OrderByDescending(t => t.TableTop) // Сортируем сверху вниз
                .ToList();

            // Находим текущую таблицу по Y-координате
            int currentTableIndex = -1;
            for (int i = 0; i < sortedTables.Count; i++)
            {
                var table = sortedTables[i];
                if (Math.Abs(table.TableTop - headerTop) < 10)
                {
                    currentTableIndex = i;
                    break;
                }
            }

            if (currentTableIndex >= 0 && currentTableIndex + 1 < sortedTables.Count)
            {
                // Следующая таблица находится ВЫШЕ (меньший Y) или НИЖЕ?
                // На самом деле следующая таблица будет НИЖЕ на странице, значит у нее МЕНЬШИЙ Top
                var nextTable = sortedTables[currentTableIndex + 1];
                nextTableTop = nextTable.TableTop;
                Console.WriteLine($"Следующая таблица найдена на Y={nextTableTop:F1}");
            }

            // Выбираем слова, которые относятся к этой таблице
            // Слова должны быть НИЖЕ заголовка (меньший Top) и ВЫШЕ следующей таблицы (больший Top)
            var tableWords = allWords.Where(w =>
                w.BoundingBox.Top < headerBottom - 1 && // НИЖЕ заголовка (меньший Y)
                (nextTableTop == null || w.BoundingBox.Top > nextTableTop.Value - 5) && // ВЫШЕ следующей таблицы (больший Y)
                                                                                         // Проверяем горизонтальные границы
                w.BoundingBox.Left >= columns.Values.Min(c => c.minX) - 5 &&
                w.BoundingBox.Right <= columns.Values.Max(c => c.maxX) + 5
            ).ToList();

            Console.WriteLine($"Найдено слов для таблицы: {tableWords.Count}");

            // Парсим данные таблицы
            ParseTableData(tableWords, columns, results);

            // Устанавливаем связи между игроками в командах
            EstablishTeamRelationships(results);

            return results;
        }

        private Dictionary<string, (double minX, double maxX, double minY, double maxY)> DetectColumnsFromHeaders(List<PdfTextProcessor.TextLine> headerLines)
        {
            var columns = new Dictionary<string, (double minX, double maxX, double minY, double maxY)>();

            // Собираем все слова заголовков
            var headerWords = headerLines.SelectMany(h => h.Words).ToList();

            foreach (var word in headerLines)
            {
                string headerText = word.Text;

                // Определяем тип колонки по тексту
                string columnKey = headerText switch
                {
                    string s when s.Contains("место", StringComparison.OrdinalIgnoreCase) => "место",
                    string s when s.Contains("Звание/", StringComparison.OrdinalIgnoreCase) => "Звание/",
                    string s when s.Contains("ФАМИЛИЯ", StringComparison.OrdinalIgnoreCase) => "ФАМИЛИЯ",
                    string s when s.Contains("Субъект", StringComparison.OrdinalIgnoreCase) => "Субъект",
                    string s when s.Contains("1 игра", StringComparison.OrdinalIgnoreCase) => "1 игра",
                    string s when s.Contains("2 игра", StringComparison.OrdinalIgnoreCase) => "2 игра",
                    string s when s.Contains("3 игра", StringComparison.OrdinalIgnoreCase) => "3 игра",
                    string s when s.Contains("4 игра", StringComparison.OrdinalIgnoreCase) => "4 игра",
                    string s when s.Contains("5 игра", StringComparison.OrdinalIgnoreCase) => "5 игра",
                    string s when s.Contains("6 игра", StringComparison.OrdinalIgnoreCase) => "6 игра",
                    string s when s.Contains("сумма 6", StringComparison.OrdinalIgnoreCase) => "сумма 6",
                    string s when s.Contains("средний", StringComparison.OrdinalIgnoreCase) => "средний",
                    _ => null
                };

                if (columnKey != null && !columns.ContainsKey(columnKey))
                {
                    // Определяем границы колонки
                    double minX = word.MinX - 5;
                    double maxX = word.MaxX + 5;
                    double minY = headerLines.Min(h => h.MinY);
                    double maxY = headerLines.Max(h => h.MaxY);

                    columns.Add(columnKey, (minX, maxX, minY, maxY));
                }
            }

            // Если есть специфичные колонки типа "ФАМИЛИЯ" и "Звание/", корректируем их границы
            if (columns.ContainsKey("ФАМИЛИЯ") && columns.ContainsKey("Звание/"))
            {
                var rankColumn = columns["Звание/"];
                var nameColumn = columns["ФАМИЛИЯ"];

                // Смещаем начало колонки "ФАМИЛИЯ" после колонки "Звание/"
                //columns["ФАМИЛИЯ"] = (rankColumn.maxX + 2, nameColumn.maxX, nameColumn.minY, nameColumn.maxY);
                columns["ФАМИЛИЯ"] = (rankColumn.maxX , nameColumn.maxX + nameColumn.minX - rankColumn.maxX + 5, nameColumn.minY, nameColumn.maxY);
            }
            if (columns.ContainsKey("Субъект") && columns.ContainsKey("1 игра"))
            {
                var game1Column = columns["1 игра"];
                var subjectColumn = columns["Субъект"];
                columns["Субъект"] = (subjectColumn.minX - (game1Column.minX - 5 - subjectColumn.maxX), game1Column.minX - 5, subjectColumn.minY, subjectColumn.maxY);
            }
            return columns;
        }

        private void ParseTableData(List<Word> words, Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns, List<PlayerResult> results)
        {
            if (!words.Any() || !columns.Any())
                return;

            // Группируем слова по строкам
            var lines = GroupWordsByLines(words);

            // Находим все строки с местами
            var placeLines = new List<(int Place, double CenterY, double MinY, double MaxY, List<Word> Line)>();

            foreach (var line in lines)
            {
                var placeWord = line.FirstOrDefault(w =>
                    w.BoundingBox.Left >= columns["место"].minX &&
                    w.BoundingBox.Left <= columns["место"].maxX &&
                    int.TryParse(w.Text, out _));

                if (placeWord != null && int.TryParse(placeWord.Text, out int place))
                {
                    double minY = line.Min(w => w.BoundingBox.Top);
                    double maxY = line.Max(w => w.BoundingBox.Bottom);
                    double centerY = (minY + maxY) / 2;

                    placeLines.Add((place, centerY, minY, maxY, line));
                }
            }

            // Если нашли места, распределяем строки по командам
            if (placeLines.Any())
            {
                // Сортируем по Y
                placeLines = placeLines.OrderBy(p => p.CenterY).ToList();

                // Вычисляем среднее расстояние между центрами строк с местами
                double avgDistance = 20.0; // значение по умолчанию
                if (placeLines.Count >= 2)
                {
                    double totalDistance = 0;
                    for (int i = 1; i < placeLines.Count; i++)
                    {
                        totalDistance += Math.Abs(placeLines[i].CenterY - placeLines[i - 1].CenterY);
                    }
                    avgDistance = totalDistance / (placeLines.Count - 1);
                }

                // Определяем диапазон для каждой команды
                var teamRanges = new List<(int Place, double MinRange, double MaxRange, double CenterY)>();

                foreach (var placeLine in placeLines)
                {
                    double range = avgDistance * 0.5; // 40% от среднего расстояния
                    double minRange = placeLine.CenterY - range;
                    double maxRange = placeLine.CenterY + range;

                    teamRanges.Add((placeLine.Place, minRange, maxRange, placeLine.CenterY));
                }

                // Распределяем все строки по командам
                var teamAssignments = new Dictionary<int, List<List<Word>>>();

                // Инициализируем команды строками с местами
                foreach (var placeLine in placeLines)
                {
                    teamAssignments[placeLine.Place] = new List<List<Word>> { placeLine.Line };
                }

                // Распределяем остальные строки
                foreach (var line in lines)
                {
                    // Пропускаем строки, которые уже в командах
                    if (placeLines.Any(p => p.Line == line))
                        continue;

                    double lineCenterY = line.Average(w => w.BoundingBox.Top);

                    // Ищем подходящую команду
                    var suitableTeams = teamRanges
                        .Where(r => lineCenterY >= r.MinRange && lineCenterY <= r.MaxRange)
                        .ToList();

                    if (suitableTeams.Count == 1)
                    {
                        // Однозначно определяем команду
                        teamAssignments[suitableTeams[0].Place].Add(line);
                    }
                    else if (suitableTeams.Count > 1)
                    {
                        // Если несколько подходящих команд, выбираем ближайшую по центру
                        var nearestTeam = suitableTeams
                            .OrderBy(t => Math.Abs(lineCenterY - t.CenterY))
                            .First();
                        teamAssignments[nearestTeam.Place].Add(line);
                    }
                    else
                    {
                        // Не нашли подходящую команду - это индивидуальный игрок
                        // Пытаемся определить его место из данных
                        int individualPlace = FindIndividualPlace(line, columns, teamAssignments);
                        teamAssignments[individualPlace] = new List<List<Word>> { line };
                    }
                }
                // Пройти по всем ключам словаря
                foreach (var key in teamAssignments.Keys.ToList())
                {
                    // Отфильтровать подсписки, оставив только те, где количество элементов >= 3
                    teamAssignments[key] = teamAssignments[key]
                        .Where(sublist => sublist.Count >= 3)
                        .ToList();
                }
                // Сохраняем все команды
                foreach (var team in teamAssignments)
                {
                    SaveTeam(team.Value, team.Key, columns, results);
                }
            }
            else
            {
                // Не нашли мест - сохраняем все как отдельные строки
                for (int i = 0; i < lines.Count; i++)
                {
                    SaveTeam(new List<List<Word>> { lines[i] }, i + 1, columns, results);
                }
            }
        }

        private int FindIndividualPlace(List<Word> line, Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns,
            Dictionary<int, List<List<Word>>> existingTeams)
        {
            // Пытаемся найти место в строке
            var placeWord = line.FirstOrDefault(w =>
                w.BoundingBox.Left >= columns["место"].minX &&
                w.BoundingBox.Left <= columns["место"].maxX &&
                int.TryParse(w.Text, out _));

            if (placeWord != null && int.TryParse(placeWord.Text, out int place))
            {
                return place;
            }

            // Если место не указано, ищем следующее свободное
            int maxPlace = existingTeams.Keys.Any() ? existingTeams.Keys.Max() : 0;
            return maxPlace + 1;
        }

        private void SaveTeam(List<List<Word>> teamLines, int teamPlace,
            Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns,
            List<PlayerResult> results)
        {
            if (!teamLines.Any())
                return;

            bool isTeam = teamLines.Count > 1;

            foreach (var line in teamLines)
            {
                var player = ParseLineToPlayerResult(line, columns);
                player.Place = teamPlace;
                player.IsTeam = isTeam;
                results.Add(player);
            }
        }

        private void EstablishTeamRelationships(List<PlayerResult> results)
        {
            // Группируем по месту (команде)
            var groups = results
                .GroupBy(p => p.Place)
                .ToList();

            foreach (var group in groups)
            {
                var players = group.ToList();

                // Определяем, является ли это командой (больше 1 игрока с одинаковым местом)
                bool isTeam = players.Count > 1;

                string teamId = isTeam ? $"Team_{group.Key}" : $"Individual_{group.Key}";
                string teamName = isTeam ? $"Команда #{group.Key}" : "Индивидуальный участник";

                // Для команд добавляем регион в название
                if (isTeam)
                {
                    var firstPlayer = players.FirstOrDefault(p => !string.IsNullOrEmpty(p.Region));
                    if (firstPlayer != null)
                    {
                        teamName = $"{firstPlayer.Region} (Команда #{group.Key})";
                    }
                }

                foreach (var player in players)
                {
                    player.IsTeam = isTeam;
                    player.TeamId = teamId;
                    player.TeamName = teamName;

                    if (isTeam)
                    {
                        // Устанавливаем связи между игроками команды
                        player.TeamMembers = players
                            .Where(p => p != player)
                            .Select(p => p.FullName)
                            .ToList();

                        player.TeamMemberIds = players
                            .Where(p => p != player)
                            .Select(p => GetPlayerId(p))
                            .ToList();
                    }
                }
            }
        }

        private PlayerResult ParseLineToPlayerResult(List<Word> line, Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns)
        {
            var result = new PlayerResult();

            foreach (var word in line)
            {
                foreach (var column in columns)
                {
                    if (word.BoundingBox.Left >= column.Value.minX &&
                        word.BoundingBox.Left <= column.Value.maxX)
                    {
                        UpdateColumn(result, column.Key, word.Text);
                        break;
                    }
                }
            }

            return result;
        }

        private string GetPlayerId(PlayerResult player)
        {
            return $"{player.FullName}_{player.Region}_{player.Place}".GetHashCode().ToString();
        }

        private List<List<Word>> GroupWordsByLines(List<Word> words)
        {
            // Группируем слова по строкам с учетом Y-координат
            var orderedWords = words.OrderBy(x => x, new PdfWordComparer()).ToList();

            var lines = new List<List<Word>>();
            List<Word> currentLine = null;
            double? lastY = null;

            foreach (var word in orderedWords)
            {
                double midY = word.BoundingBox.Top + word.BoundingBox.Height / 2;

                if (currentLine == null || (lastY.HasValue && Math.Abs(midY - lastY.Value) > 5))
                {
                    // Новая строка
                    if (currentLine != null)
                    {
                        lines.Add(currentLine);
                    }
                    currentLine = new List<Word> { word };
                }
                else
                {
                    // Текущая строка
                    currentLine.Add(word);
                }

                lastY = midY;
            }

            if (currentLine != null && currentLine.Any())
            {
                lines.Add(currentLine);
            }

            return lines;
        }

        private void UpdateColumn(PlayerResult row, string columnName, string value)
        {
            string[] switchStrings = { "место", "Звание/", "ФАМИЛИЯ", "Субъект", "1 игра", "2 игра", "3 игра", "4 игра", "5 игра", "6 игра", "сумма 6", "средний" };

            var match = switchStrings.FirstOrDefault(s => columnName.Contains(s));

            switch (match)
            {
                case "место":
                    // Место уже устанавливается в SaveTeam
                    break;
                case "Звание/":
                    row.Rank = (row.Rank + " " + value).Trim();
                    break;
                case "ФАМИЛИЯ":
                    row.FullName = (row.FullName + " " + value).Trim();
                    break;
                case "Субъект":
                    row.Region = (row.Region + " " + value).Trim();
                    break;
                case "1 игра":
                    if (int.TryParse(value, out int game1)) row.Game1 = game1;
                    break;
                case "2 игра":
                    if (int.TryParse(value, out int game2)) row.Game2 = game2;
                    break;
                case "3 игра":
                    if (int.TryParse(value, out int game3)) row.Game3 = game3;
                    break;
                case "4 игра":
                    if (int.TryParse(value, out int game4)) row.Game4 = game4;
                    break;
                case "5 игра":
                    if (int.TryParse(value, out int game5)) row.Game5 = game5;
                    break;
                case "6 игра":
                    if (int.TryParse(value, out int game6)) row.Game6 = game6;
                    break;
                
            }
        }
        private List<PlayerResult> FilterValidResults(List<PlayerResult> results)
        {
            return results
                .Where(player =>
                    // Есть полное имя (минимум 2 слова)
                    !string.IsNullOrWhiteSpace(player.FullName) &&
                    player.FullName.Trim().Split(' ').Length >= 2 &&

                    // Не является заголовком
                    !IsHeaderOrMetadata(player) &&

                    // Для командных игроков: проверяем наличие результатов
                    (player.IsTeam ||(
                     // Для индивидуальных: должен быть хотя бы один результат
                     player.Game1 > 0 && player.Game2 > 0 ))
                )
                .ToList();
        }

        private bool IsHeaderOrMetadata(PlayerResult player)
        {
            // Быстрая проверка на заголовок
            string text = (player.FullName + " " + player.Region + " " + player.Rank).ToLower();

            return text.Contains("место") || text.Contains("звание") ||
                   text.Contains("фамилия") || text.Contains("субъект") ||
                   text.Contains("игра") || text.Contains("всего") ||
                   text.Contains("сумма") || text.Contains("средний");
        }
    }

    public class PlayerResult
    {
        public int Place { get; set; }
        public string Rank { get; set; } = string.Empty;
        //public string FullName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        // Результаты игр
        public int Game1 { get; set; }
        public int Game2 { get; set; }
        public int Game3 { get; set; }
        public int Game4 { get; set; }
        public int Game5 { get; set; }
        public int Game6 { get; set; }

        // Вычисляемые свойства вместо полей
        public int Total => CalculateTotal();
        public double Average => CalculateAverage();

        // Если нужно сохранять оригинальные значения из таблицы (для капитанов команд)
        private int? _originalTotal;
        private double? _originalAverage;

        // Информация о команде
        public bool IsTeam { get; set; }
        public string TeamId { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public List<string> TeamMembers { get; set; } = new List<string>();
        public List<string> TeamMemberIds { get; set; } = new List<string>();

        // Дополнительные вычисляемые свойства
        public int TeamSize => TeamMembers.Count + 1;
        // Добавляем свойство для пола
        public Gender Gender { get; private set; }

        public void DetermineGender()
        {
            // Для спортивных таблиц используем специальный детектор
            this.Gender = SportsGenderDetector.DetectFromSportsTable(this.FullName);
        }

        // Или автоматически при установке имени
        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                this.Gender = SportsGenderDetector.DetectFromSportsTable(value);
            }
        }

        // Вспомогательные свойства
        public bool IsMale => Gender == Gender.Male;
        public bool IsFemale => Gender == Gender.Female;
        public string GenderDisplay => Gender switch
        {
            Gender.Male => "Мужчина",
            Gender.Female => "Женщина",
            _ => "Не определен"
        };
        // Методы для вычисления
        private int CalculateTotal()
        {
            // Если есть сохраненное оригинальное значение (для капитанов команд), используем его
            if (_originalTotal.HasValue)
                return _originalTotal.Value;

            // Иначе вычисляем сумму всех игр
            return Game1 + Game2 + Game3 + Game4 + Game5 + Game6;
        }

        private double CalculateAverage()
        {
            // Если есть сохраненное оригинальное значение (для капитанов команд), используем его
            if (_originalAverage.HasValue)
                return _originalAverage.Value;

            // Иначе вычисляем среднее
            int total = CalculateTotal();
            int gamesCount = CountGamesWithResults();

            return gamesCount > 0 ? Math.Round((double)total / gamesCount, 2) : 0;
        }

        private int CountGamesWithResults()
        {
            int count = 0;
            if (Game1 > 0) count++;
            if (Game2 > 0) count++;
            if (Game3 > 0) count++;
            if (Game4 > 0) count++;
            if (Game5 > 0) count++;
            if (Game6 > 0) count++;
            return count;
        }

        // Методы для установки оригинальных значений (если нужно сохранить из таблицы)
        public void SetOriginalTotal(int total)
        {
            _originalTotal = total;
        }

        public void SetOriginalAverage(double average)
        {
            _originalAverage = Math.Round(average, 2);
        }

        // Метод для получения всех результатов игр в виде массива
        public int[] GetAllGameResults() => new[] { Game1, Game2, Game3, Game4, Game5, Game6 };

        // Метод для получения количества сыгранных игр
        public int PlayedGamesCount => GetAllGameResults().Count(score => score > 0);

        // Метод для получения максимального результата
        public int BestGameScore => GetAllGameResults().Max();

        // Метод для получения минимального результата (среди сыгранных)
        public int WorstGameScore
        {
            get
            {
                var playedGames = GetAllGameResults().Where(score => score > 0).ToList();
                return playedGames.Any() ? playedGames.Min() : 0;
            }
        }

        public string GetTeamInfo()
        {
            if (!IsTeam) return "Индивидуальный участник";
            var members = string.Join(", ", TeamMembers);
            return $"Команда: {TeamName}, Место: {Place}, Состав ({TeamSize} чел.): {members}";
        }

        // Метод для отладки
        public string GetDetailedInfo()
        {
            return $"{FullName} - Место: {Place}, Игры: {Game1}/{Game2}/{Game3}/{Game4}/{Game5}/{Game6}, " +
                   $"Всего: {Total}, Среднее: {Average:F2}";
        }
    }

    public class PdfWordComparer : IComparer<Word>
    {
        private readonly double _lineTolerance;
        public PdfWordComparer(double lineTolerance = 3.0)
        {
            _lineTolerance = lineTolerance;
        }

        public int Compare(Word x, Word y)
        {
            double midY1 = x.BoundingBox.Top + x.BoundingBox.Height / 2;
            double midY2 = y.BoundingBox.Top + y.BoundingBox.Height / 2;

            if (Math.Abs(midY1 - midY2) > _lineTolerance)
            {
                return midY2.CompareTo(midY1);
            }

            return x.BoundingBox.Left.CompareTo(y.BoundingBox.Left);
        }
    }

    public class LineWordComparer : IEqualityComparer<Word>
    {
        public bool Equals(Word a, Word b)
        {
            return Math.Abs(a.BoundingBox.Bottom - b.BoundingBox.Bottom) < 1.0;
        }

        public int GetHashCode(Word w)
        {
            return 0;
        }
    }

    public class PdfTextProcessor
    {
        public static IEnumerable<TextLine> GetTextLines(Page page)
        {
            const double spaceThreshold = 5.0;
            var words = page.GetWords().OrderBy(x => x, new PdfWordComparer()).ToList();

            var lineGroups = words
                .GroupBy(w => w, new LineWordComparer());

            foreach (var lineGroup in lineGroups)
            {
                var lineWords = lineGroup
                    .OrderBy(w => w.BoundingBox.Left)
                    .ToList();

                var currentLine = new TextLine();

                foreach (var word in lineWords)
                {
                    if (currentLine.Words.Count == 0)
                    {
                        currentLine.AddWord(word);
                        continue;
                    }

                    var lastWord = currentLine.Words.Last();
                    var space = word.BoundingBox.Left - lastWord.BoundingBox.Right;

                    if (space <= spaceThreshold && space >= 0)
                    {
                        currentLine.AddWord(word, " ");
                    }
                    else
                    {
                        yield return currentLine;
                        currentLine = new TextLine();
                        currentLine.AddWord(word);
                    }
                }

                if (currentLine.Words.Any())
                    yield return currentLine;
            }
        }

        public static List<TextLine> GetTextLinesFromWords(List<Word> words)
        {
            const double spaceThreshold = 5.0;
            var orderedWords = words.OrderBy(x => x, new PdfWordComparer()).ToList();

            var result = new List<TextLine>();
            var lineGroups = orderedWords
                .GroupBy(w => w, new LineWordComparer());

            foreach (var lineGroup in lineGroups)
            {
                var lineWords = lineGroup
                    .OrderBy(w => w.BoundingBox.Left)
                    .ToList();

                var currentLine = new TextLine();

                foreach (var word in lineWords)
                {
                    if (currentLine.Words.Count == 0)
                    {
                        currentLine.AddWord(word);
                        continue;
                    }

                    var lastWord = currentLine.Words.Last();
                    var space = word.BoundingBox.Left - lastWord.BoundingBox.Right;

                    if (space <= spaceThreshold && space >= 0)
                    {
                        currentLine.AddWord(word, " ");
                    }
                    else
                    {
                        result.Add(currentLine);
                        currentLine = new TextLine();
                        currentLine.AddWord(word);
                    }
                }

                if (currentLine.Words.Any())
                    result.Add(currentLine);
            }

            return result;
        }

        public class TextLine
        {
            public string Text { get; private set; } = string.Empty;
            public List<Word> Words { get; } = new List<Word>();
            public double MinY => Words.Min(w => w.BoundingBox.Bottom);
            public double MaxY => Words.Max(w => w.BoundingBox.Top);
            public double MinX => Words.Min(w => w.BoundingBox.Left);
            public double MaxX => Words.Max(w => w.BoundingBox.Right);

            public void AddWord(Word word, string separator = "")
            {
                Words.Add(word);
                Text += separator + word.Text;
            }
        }
    }
    public static class GenderDetector
    {
        // Списки женских окончаний
        private static readonly string[] FemaleEndings =
        {
        "а", "я", "ья", "ина", "ова", "ева", "ская", "цкая",
        "на", "ла", "ра", "та", "ва", "га", "ка", "ша"
    };

        // Списки мужских окончаний
        private static readonly string[] MaleEndings =
        {
        "ий", "ой", "ой", "ов", "ев", "ин", "ын", "ский", "цкий",
        "он", "ен", "ун", "ан", "ян", "ль", "рь", "др", "тр"
    };

        // Очевидные мужские имена
        private static readonly string[] MaleFirstNames =
        {
        "Александр", "Сергей", "Владимир", "Андрей", "Алексей",
        "Дмитрий", "Максим", "Иван", "Михаил", "Артем",
        "Николай", "Евгений", "Павел", "Роман", "Виктор",
        "Олег", "Юрий", "Игорь", "Вадим", "Глеб",
        "Станислав", "Василий", "Валерий", "Борис", "Георгий",
        "Петр", "Константин", "Федор", "Яков", "Тимофей", "Рустам"
    };

        // Очевидные женские имена
        private static readonly string[] FemaleFirstNames =
        {
        "Анна", "Елена", "Ольга", "Наталья", "Марина",
        "Ирина", "Светлана", "Татьяна", "Екатерина", "Юлия",
        "Александра", "Мария", "Дарья", "Анастасия", "Виктория",
        "Ксения", "Людмила", "Галина", "Валентина", "Надежда",
        "Лариса", "Зоя", "Вера", "Нина", "Лидия",
        "София", "Алиса", "Евгения", "Валерия", "Полина"
    };

        // Патронимы (отчества) - наиболее надежный признак
        private static readonly string[] MalePatronymics =
        {
        "ович", "евич", "ич"
    };

        private static readonly string[] FemalePatronymics =
        {
        "овна", "евна", "ична", "инична"
    };

        public static Gender DetectGender(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return Gender.Unknown;

            string name = fullName.Trim();
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 1. Проверка по отчеству (самый надежный признак)
            if (parts.Length >= 2)
            {
                string possiblePatronymic = parts[1].ToLower();

                foreach (var ending in MalePatronymics)
                {
                    if (possiblePatronymic.EndsWith(ending))
                        return Gender.Male;
                }

                foreach (var ending in FemalePatronymics)
                {
                    if (possiblePatronymic.EndsWith(ending))
                        return Gender.Female;
                }
            }

            // 2. Проверка по первому имени
            if (parts.Length >= 1)
            {
                string firstName = parts[0];

                // Проверка по списку имен
                if (MaleFirstNames.Contains(firstName, StringComparer.OrdinalIgnoreCase))
                    return Gender.Male;

                if (FemaleFirstNames.Contains(firstName, StringComparer.OrdinalIgnoreCase))
                    return Gender.Female;

                // Проверка по окончаниям имени
                string firstNameLower = firstName.ToLower();

                // Женские окончания имен
                if (firstNameLower.EndsWith("а") || firstNameLower.EndsWith("я") ||
                    firstNameLower.EndsWith("ина") || firstNameLower.EndsWith("ла"))
                {
                    // Исключения (мужские имена с женскими окончаниями)
                    var maleExceptions = new[] { "илья", "кузьма", "никита", "савва", "фома" };
                    if (!maleExceptions.Contains(firstNameLower))
                        return Gender.Female;
                }

                // Мужские окончания имен
                if (firstNameLower.EndsWith("ий") || firstNameLower.EndsWith("ей") ||
                    firstNameLower.EndsWith("ль") || firstNameLower.EndsWith("н"))
                {
                    return Gender.Male;
                }
            }

            // 3. Проверка по фамилии (менее надежно)
            if (parts.Length >= 1)
            {
                string lastName = parts[parts.Length - 1].ToLower();

                // Женские окончания фамилий
                foreach (var ending in FemaleEndings)
                {
                    if (lastName.EndsWith(ending))
                    {
                        // Проверка на мужские фамилии с женскими окончаниями
                        var maleExceptions = new[] { "донской", "толстой", "сухой", "пушной" };
                        if (!maleExceptions.Any(e => lastName.EndsWith(e)))
                            return Gender.Female;
                    }
                }

                // Мужские окончания фамилий
                foreach (var ending in MaleEndings)
                {
                    if (lastName.EndsWith(ending))
                        return Gender.Male;
                }
            }

            return Gender.Unknown;
        }

        public enum Gender
        {
            Unknown = 0,
            Male = 1,
            Female = 2
        }
    }
    public static class SportsGenderDetector
    {
        // В спортивных таблицах обычно формат "Фамилия Имя"
        public static Gender DetectFromSportsTable(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return Gender.Unknown;

            var parts = playerName.Trim()
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();

            // В спорте обычно: "Иванов Иван" или "Иванов Иван Иванович"

            // 1. Если есть отчество - самый надежный способ
            foreach (var part in parts)
            {
                if (IsPatronymic(part, out Gender gender))
                    return gender;
            }

            // 2. Анализируем второе слово (скорее всего, это имя)
            if (parts.Length >= 2)
            {
                return AnalyzeRussianName(parts[1]); // Второе слово - имя
            }

            // 3. Анализируем первое слово (фамилия)
            if (parts.Length >= 1)
            {
                return AnalyzeRussianLastName(parts[0]);
            }

            return Gender.Unknown;
        }

        private static bool IsPatronymic(string word, out Gender gender)
        {
            gender = Gender.Unknown;
            string lower = word.ToLower();

            if (lower.EndsWith("ович") || lower.EndsWith("евич") || lower.EndsWith("ич"))
            {
                gender = Gender.Male;
                return true;
            }

            if (lower.EndsWith("овна") || lower.EndsWith("евна") || lower.EndsWith("ична"))
            {
                gender = Gender.Female;
                return true;
            }

            return false;
        }

        private static Gender AnalyzeRussianName(string name)
        {
            // Список явно мужских имен из спорта
            var maleSportNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Александр", "Сергей", "Владимир", "Андрей", "Алексей",
            "Дмитрий", "Максим", "Иван", "Михаил", "Артем",
            "Николай", "Евгений", "Павел", "Роман", "Виктор",
            "Олег", "Юрий", "Игорь", "Вадим", "Глеб",
            "Станислав", "Василий", "Валерий", "Борис"
        };

            // Список явно женских имен из спорта
            var femaleSportNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Анна", "Елена", "Ольга", "Наталья", "Марина",
            "Ирина", "Светлана", "Татьяна", "Екатерина", "Юлия",
            "Александра", "Мария", "Дарья", "Анастасия", "Виктория",
            "Ксения", "Людмила", "Галина", "Валентина", "Надежда"
        };

            if (maleSportNames.Contains(name))
                return Gender.Male;

            if (femaleSportNames.Contains(name))
                return Gender.Female;

            // Эвристики
            string lower = name.ToLower();

            if (lower.EndsWith("а") || lower.EndsWith("я"))
                return Gender.Female;

            if (lower.EndsWith("ий") || lower.EndsWith("ей"))
                return Gender.Male;

            return Gender.Unknown;
        }

        private static Gender AnalyzeRussianLastName(string lastName)
        {
            string lower = lastName.ToLower();

            // Мужские окончания фамилий
            if (lower.EndsWith("ов") || lower.EndsWith("ев") || lower.EndsWith("ин"))
                return Gender.Male;

            // Женские окончания фамилий
            if (lower.EndsWith("ова") || lower.EndsWith("ева") || lower.EndsWith("ина"))
                return Gender.Female;

            return Gender.Unknown;
        }
    }
}