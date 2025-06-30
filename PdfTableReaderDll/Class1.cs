using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PdfTableReaderDll
{
    public class PdfTableReader
    {
        public List<PlayerResult> Results { get; set; } = new List<PlayerResult>();
        public string FileName { get; set; }
        public PdfTableReader(string fileName) { FileName = fileName; }
        public List<PlayerResult> LoadDataFromPdf()
        {
            using var document = PdfDocument.Open(FileName);
            var page = document.GetPage(1);
            // Заменяем page.GetWords() на наш метод
            var textLines = PdfTextProcessor.GetTextLines(page).ToList();
            var words = textLines.SelectMany(l => l.Words).ToList();


            // 1. Находим заголовки таблицы
            var headerWords = DetectHeaderWords(textLines);
            //var headerWords2 = headerWords.SelectMany(l => l.Words).ToList();
            // 2. Определяем границы столбцов
            //var columns = DetectColumns(headerWords2);
            Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns = new Dictionary<string, (double minX, double maxX, double minY, double maxY)>();
            foreach (var header in headerWords)
            {
                if (!columns.ContainsKey(header.Text))
                {
                    if (header.Text.Contains("ФАМИЛИЯ"))
                    {
                        var rank = headerWords.FirstOrDefault(o => o.Text == "Звание/");
                        if (rank != null)
                        {
                            columns.Add(header.Text, (minX: rank.MaxX + 5, maxX: header.MaxX + header.MinX - rank.MaxX, minY: header.MinY, maxY: header.MaxY));
                        }
                        else
                        {
                            columns.Add(header.Text, (minX: header.MinX, maxX: header.MaxX, minY: header.MinY, maxY: header.MaxY));
                        }

                    }
                    else if (header.Text.ToLower() == "Субъект".ToLower())
                    {
                        var game1 = headerWords.FirstOrDefault(o => o.Text == "1 игра");
                        if (game1 != null)
                        {
                            columns.Add(header.Text, (minX: header.MinX - (game1.MinX - header.MaxX), maxX: game1.MinX - 5, minY: header.MinY, maxY: header.MaxY));
                        }
                        else
                        {
                            columns.Add(header.Text, (minX: header.MinX, maxX: header.MaxX, minY: header.MinY, maxY: header.MaxY));
                        }
                    }
                    else
                    {
                        columns.Add(header.Text, (minX: header.MinX, maxX: header.MaxX, minY: header.MinY, maxY: header.MaxY));
                    }
                }


            }


            // 3. Парсим данные таблицы
            ParseTableData(words, columns);
            return Results;
        }

        private List<PdfTextProcessor.TextLine> DetectHeaderWords(List<PdfTextProcessor.TextLine> textLines)
        {
            // Ищем строку с основными заголовками
            var headerTitles = new[] { "место", "Звание/", "ФАМИЛИЯ", "Субъект", "1 игра", "2 игра", "3 игра", "4 игра", "5 игра", "6 игра", "сумма 6", "средний" };

            return textLines
                .Where(w => headerTitles.Any(t => w.Text.StartsWith(t, StringComparison.OrdinalIgnoreCase)))
                //.GroupBy(w => w.MinY)
                //.OrderByDescending(g => g.Key)
                //.FirstOrDefault()?
                .ToList();
        }

        private Dictionary<string, (double minX, double maxX, double minY, double maxY)> DetectColumns(List<Word> headerWords)
        {
            var columns = new Dictionary<string, (double minX, double maxX, double minY, double maxY)>();
            var orderedHeaders = headerWords.OrderBy(w => w.BoundingBox.Left).ToList();

            // Определяем границы для каждого заголовка
            foreach (var header in orderedHeaders)
            {
                var columnName = header.Text switch
                {
                    "место" => "Place",
                    "Звание/" => "Rank",
                    "ФАМИЛИЯ ИМЯ" => "Name",
                    "Субъект" => "Region",
                    "1 игра" => "Game1",
                    "2 игра" => "Game2",
                    "3 игра" => "Game3",
                    "4 игра" => "Game4",
                    "5 игра" => "Game5",
                    "6 игра" => "Game6",
                    "сумма 6" => "Total",
                    "средний" => "Average",
                    _ => null
                };

                if (columnName != null)
                {
                    var bounds = (header.BoundingBox.Left - 5, header.BoundingBox.Right + 5, header.BoundingBox.Top + 2, header.BoundingBox.Bottom + 2);
                    columns[columnName] = bounds;
                }
            }

            // Добавляем остальные игровые столбцы
            var gameColumns = columns.Where(c => c.Key.StartsWith("Game")).ToList();
            for (int i = 2; i <= 6; i++)
            {
                var prevColumn = columns[$"Game{i - 1}"];
                var newColumn = (prevColumn.minX + 10, prevColumn.maxX + 60, prevColumn.minY, prevColumn.maxY);
                columns[$"Game{i}"] = newColumn;
            }

            // Добавляем Total и Average
            var lastGame = columns["Game6"];
            columns["Total"] = (lastGame.maxX + 10, lastGame.maxX + 60, lastGame.minY, lastGame.maxY);
            columns["Average"] = (columns["Total"].Item2 + 10, columns["Total"].Item2 + 60, lastGame.minY, lastGame.maxY);

            return columns;
        }

        private void ParseTableData(List<Word> words, Dictionary<string, (double minX, double maxX, double minY, double maxY)> columns)
        {
            // Находим Y-координату начала данных
            var headerBottom = columns.Values.Max(c => c.maxY);
            var dataWords = words.Where(w => w.BoundingBox.Top < headerBottom - 20).ToList();

            PlayerResult currentRow = null;
            var wordsOrdered = dataWords.OrderBy(x => x, new PdfWordComparer()).ToList();

            // Группировка по строкам
            //var lineGroups = words
            //    .GroupBy(w => w, new LineWordComparer());
            int i = 0;
            foreach (var word in wordsOrdered)
            {
                if (IsNewRow(word, columns["место"]))
                {
                    if (currentRow != null) Results.Add(currentRow);
                    currentRow = new PlayerResult();
                    i = 2;
                }
                else if (IsNewRow(word, columns["Звание/"]) && i <= 0)
                {
                    if (currentRow != null) Results.Add(currentRow);
                    currentRow = new PlayerResult();
                }

                if (currentRow == null) continue;

                foreach (var column in columns)
                {
                    if (word.BoundingBox.Left >= column.Value.minX &&
                        word.BoundingBox.Left <= column.Value.maxX)
                    {
                        UpdateColumn(currentRow, column.Key, word.Text);
                        break;
                    }
                }
                i -= 1;
            }

            if (currentRow != null) Results.Add(currentRow);
        }

        private bool IsNewRow(Word word, (double minX, double maxX, double minY, double maxY) placeColumn)
        {
            return word.BoundingBox.Left >= placeColumn.minX &&
                   word.BoundingBox.Left <= placeColumn.maxX; //&&
                                                              //int.TryParse(word.Text, out _);
        }

        private void UpdateColumn(PlayerResult row, string columnName, string value)
        {
            string[] switchStrings = { "место", "Звание/", "ФАМИЛИЯ", "Субъект", "1 игра", "2 игра", "3 игра", "4 игра", "5 игра", "6 игра", "сумма 6", "средний" };

            switch (switchStrings.FirstOrDefault<string>(s => columnName.Contains(s)))
            {
                case "место":
                    if (int.TryParse(value, out int place)) row.Place = place;
                    break;
                case "Звание/":
                    row.Rank += " " + value;
                    break;
                case "ФАМИЛИЯ":
                    row.FullName += " " + value;
                    break;
                case "Субъект":
                    row.Region += " " + value;
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
                case "сумма 6":
                    if (int.TryParse(value, out int total)) row.Total = total;
                    break;
                case "средний":
                    if (double.TryParse(value.Replace(',', '.'), out double avg)) row.Average = avg;
                    break;
            }
        }
    }
    public class PlayerResult
    {
        public int Place { get; set; }
        public string Rank { get; set; }
        public string FullName { get; set; }
        public string Region { get; set; }
        public int Game1 { get; set; }
        public int Game2 { get; set; }
        public int Game3 { get; set; }
        public int Game4 { get; set; }
        public int Game5 { get; set; }
        public int Game6 { get; set; }
        public int Total { get; set; }
        public double Average { get; set; }
    }
    public class PdfWordComparer : IComparer<Word>
    {
        private readonly double _lineTolerance;
        public PdfWordComparer(double lineTolerance = 2.0)
        {
            _lineTolerance = lineTolerance;
        }
        public int Compare(Word x, Word y)
        {
            bool sameLine = Math.Abs(x.BoundingBox.Top - y.BoundingBox.Bottom) <= _lineTolerance || Math.Abs(x.BoundingBox.Bottom - y.BoundingBox.Top) <= _lineTolerance || (x.BoundingBox.Centroid.Y <= y.BoundingBox.Top && x.BoundingBox.Centroid.Y >= y.BoundingBox.Bottom) || (y.BoundingBox.Centroid.Y <= x.BoundingBox.Top && y.BoundingBox.Centroid.Y >= x.BoundingBox.Bottom);
            if (!sameLine)
            {
                return y.BoundingBox.Top.CompareTo(x.BoundingBox.Top);
            }
            int xComparison = x.BoundingBox.Left.CompareTo(y.BoundingBox.Left);
            if (xComparison != 0)
            {
                return xComparison;
            }
            return x.BoundingBox.Width.CompareTo(y.BoundingBox.Width);
        }

    }
    public class LineWordComparer : IEqualityComparer<Word>
    {


        public bool Equals(Word a, Word b)
        {

            return Math.Abs(a.BoundingBox.Bottom - b.BoundingBox.Bottom) < 1.0;// || (a.BoundingBox.Centroid.Y <= b.BoundingBox.Top && a.BoundingBox.Centroid.Y >= b.BoundingBox.Bottom) || (b.BoundingBox.Centroid.Y <= a.BoundingBox.Top && b.BoundingBox.Centroid.Y >= a.BoundingBox.Bottom);


        }

        public int GetHashCode(Word w)
        {
            return 0;
        }


    }
    public class PdfTextProcessor
    {
        public static IEnumerable<TextLine> GetTextLines(UglyToad.PdfPig.Content.Page page)
        {
            const double spaceThreshold = 5.0; // Максимальное расстояние для пробела
            var words = page.GetWords().OrderBy(x => x, new PdfWordComparer()).ToList();

            // Группировка по строкам
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

}
