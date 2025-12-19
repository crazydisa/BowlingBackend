using System;
using System.Collections.Generic;
using System.Text;

namespace GamesResults.Utils
{
    public static class GenderDetector
    {
        public static Gender DetectGender(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return Gender.Unknown;

            // Алгоритм определения пола по ФИО
            var parts = fullName.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();

            if (parts.Length >= 2)
            {
                // Анализ по имени (второе слово в формате "Фамилия Имя")
                string firstName = parts[1];

                // Списки имен (можно расширить)
                var maleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Александр", "Сергей", "Владимир", "Андрей", "Алексей",
                "Дмитрий", "Максим", "Иван", "Михаил", "Артем",
                "Николай", "Евгений", "Павел", "Роман", "Виктор",
                "Олег", "Юрий", "Игорь", "Вадим", "Глеб",
                "Станислав", "Василий", "Валерий", "Борис", "Георгий",
                "Петр", "Константин", "Федор", "Яков", "Тимофей",
                "Рустам", "Руслан", "Равиль", "Тимур", "Данил"
            };

                var femaleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Анна", "Елена", "Ольга", "Наталья", "Марина",
                "Ирина", "Светлана", "Татьяна", "Екатерина", "Юлия",
                "Александра", "Мария", "Дарья", "Анастасия", "Виктория",
                "Ксения", "Людмила", "Галина", "Валентина", "Надежда",
                "Лариса", "Зоя", "Вера", "Нина", "Лидия"
            };

                if (maleNames.Contains(firstName))
                    return Gender.Male;

                if (femaleNames.Contains(firstName))
                    return Gender.Female;
            }

            // Дополнительная логика анализа по отчеству или окончаниям
            // ...

            return Gender.Unknown;
        }
    }
}
