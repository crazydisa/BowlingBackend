
using GamesResults.Models.Bowling;
using GamesResults.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfTableReader2;
using System;
using System.Globalization;
using System.Text.Json;

namespace GamesResults.Controllers.Upload
{
    [ApiController]

    public class UploadController : ControllerBase
    {
        private readonly AppDbContext nsContext;
        private readonly AppService service;


        public UploadController(AppDbContext appContext, AppService service)
        {
            nsContext = appContext;
            this.service = service;
        }

        [HttpPost("/universal/upload-data")]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadPdfWithData([FromForm] PdfUploadDto dto, [FromForm] IFormFile pdfFile)
        {
            // 1. Валидация входных данных (упрощенная версия)
            if (dto == null) return BadRequest(new { Title = "Данные формы не предоставлены" });
            if (string.IsNullOrWhiteSpace(dto.Tournament)) return BadRequest(new { Title = "Турнир обязателен" });
            if (dto.Year < 1900 || dto.Year > DateTime.Now.Year + 1) return BadRequest(new { Title = "Некорректный год" });
            if (string.IsNullOrWhiteSpace(dto.City)) return BadRequest(new { Title = "Город обязателен" });
            if (pdfFile == null || pdfFile.Length == 0) return BadRequest(new { Title = "PDF файл обязателен" });

            // 2. Валидация файла
            var extension = Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
            if (extension != ".pdf") return BadRequest(new { Title = "Разрешены только PDF файлы" });
            if (!pdfFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Title = "Неверный тип файла" });

            // 3. Читаем файл ОДИН раз в массив байтов
            byte[] pdfBytes;
            byte[] pdfBytes2;
            //using (var memoryStream = new MemoryStream())
            //{
            //    await pdfFile.CopyToAsync(memoryStream);
            //    pdfBytes = memoryStream.ToArray();
            //}
            // Альтернативный способ чтения файла
            using (var stream = pdfFile.OpenReadStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    pdfBytes = memoryStream.ToArray();
                }
            }
            // Проверяем размер файла
            if (pdfBytes.Length == 0)
            {
                return BadRequest(new { Title = "Файл пуст" });
            }

            // Проверяем сигнатуру PDF
            if (pdfBytes.Length < 4)
            {
                return BadRequest(new { Title = "Файл слишком мал для PDF" });
            }

            if (!(pdfBytes[0] == 0x25 && pdfBytes[1] == 0x50 &&
                  pdfBytes[2] == 0x44 && pdfBytes[3] == 0x46))
            {
                return BadRequest(new { Title = "Файл не является валидным PDF" });
            }

            // 4. Обработка PDF (используем копию байтов)
            List<List<PlayerResult>> allTables;
            using (var processingStream = new MemoryStream(pdfBytes))
            {
                var tableReader = new PdfTableReader();
                allTables = await tableReader.GetAllTablesFromPdf(processingStream, pdfFile.FileName);
            }

            if (allTables == null || !allTables.Any())
            {
                return BadRequest(new { Title = "Не удалось извлечь данные из PDF" });
            }

            // 5. Использование транзакции
            using var transaction = await nsContext.Database.BeginTransactionAsync();

            try
            {
                // 6. Подготовка данных
                var tournamentName = dto.Tournament.Trim();
                var cityName = dto.City.Trim();
                var StartDate = new DateTime(dto.Year, 1, 1);

                // 7. Проверка существования события (оптимизированная версия)
                var TournamentExists =  nsContext.Tournaments
                    .Any(e => e.Name.ToLower() == tournamentName.ToLower() &&
                                    e.StartDate.HasValue &&
                                    e.StartDate.Value.Year == dto.Year &&
                                    e.City.Title.ToLower() == cityName.ToLower());

                if (TournamentExists)
                {
                    return Conflict(new
                    {
                        Title = "Турнир уже существует",
                        Tournament = tournamentName,
                        City = cityName,
                        Year = dto.Year
                    });
                }

                // 8. Поиск или создание города
                var city =  nsContext.Cities
                    .FirstOrDefault(c => c.Title.ToLower() == cityName.ToLower());

                if (city == null)
                {
                    city = new City
                    {
                        Title = cityName,
                        Name = cityName,
                        CreatedAt = DateTime.UtcNow
                    };
                    nsContext.Cities.Add(city);
                    await nsContext.SaveChangesAsync(); // Сохраняем, чтобы получить Id
                }

                // 9. Создание события
                var turnir = new Tournament
                {
                    Title= tournamentName,
                    Name = tournamentName,
                    Description = dto.Description?.Trim(),
                    StartDate = StartDate,
                    CreatedAt = DateTime.UtcNow,
                    CityId = city.Id,
                    City = city
                };
                nsContext.Tournaments.Add(turnir);
                // ★★★★ ДОБАВЛЯЕМ СОЗДАНИЕ TournamentDocument ЗДЕСЬ ★★★★
                // Сохраняем Tournament, чтобы получить Id
                await nsContext.SaveChangesAsync();

                var TournamentDocument = new TournamentDocument
                {
                    Title = $"Турнирный файл: {dto.Tournament} {dto.Year}",
                    OriginalFileName = pdfFile.FileName,
                    ContentType = pdfFile.ContentType,
                    FileSize = pdfFile.Length,
                    FileData = pdfBytes,
                    CreatedAt = DateTime.UtcNow,
                    Description = $"Турнирный файл: {dto.Tournament} {dto.Year}",
                    DocumentType = "PDF",
                    TournamentId = turnir.Id,
                    Tournament = turnir
                };

                nsContext.TournamentDocuments.Add(TournamentDocument);

                // Инициализируем коллекцию Documents у Tournament (если нужно)
                if (turnir.Documents == null)
                {
                    turnir.Documents = new List<TournamentDocument>();
                }
                turnir.Documents.Add(TournamentDocument);

                // 10. Обработка игроков и их результатов
                var tableResults = new List<BaseTournamentResult>();
                var districtsCache = new Dictionary<string, District>();
                var playersCache = new Dictionary<string, Player>();

                //Получаем результаты игр из файла
                List<PlayerResult> allPlayers = allTables.SelectMany(table => table).ToList();
                
                // Получаем все существующие районы для кэширования
                var existingDistricts = nsContext.Districts
                    .Where(d => allPlayers.Select(p => p.Region).Contains(d.Title))
                    .ToList();

                foreach (var district in existingDistricts)
                {
                    districtsCache[district.Title] = district;
                }

                // Получаем всех существующих игроков с их районами для кэширования
                var playerNames = allPlayers.Select(p => p.FullName).Distinct().ToList();

                var existingPlayers =  nsContext.Players
                    .Where(p => playerNames.Contains(p.Name))
                    .Join(
                        nsContext.Districts,
                        player => player.DistrictId,
                        district => district.Id,
                        (player, district) => new
                        {
                            Player = player,
                            District = district
                        })
                    .ToList();

                // Заполняем кэш
                foreach (var item in existingPlayers)
                {
                    item.Player.District = item.District; // Устанавливаем связь вручную
                    playersCache[item.Player.Name] = item.Player;
                }
                await nsContext.SaveChangesAsync();
                foreach (var playerResult in allPlayers)
                {
                    // Поиск или создание района
                    District bdDistrict = null;
                    if (!string.IsNullOrWhiteSpace(playerResult.Region))
                    {
                        var regionName = playerResult.Region.Trim();

                        if (!districtsCache.TryGetValue(regionName, out bdDistrict))
                        {
                            bdDistrict =  nsContext.Districts
                                .FirstOrDefault(d => d.Title == regionName);

                            if (bdDistrict == null)
                            {
                                bdDistrict = new District
                                {
                                    Title = regionName,
                                    Name = regionName,
                                    CreatedAt = DateTime.UtcNow
                                };
                                nsContext.Districts.Add(bdDistrict);
                            }

                            districtsCache[regionName] = bdDistrict;
                        }
                    }
                    await nsContext.SaveChangesAsync();
                    // Поиск или создание игрока
                    Player bdPlayer = null;
                    var playerName = playerResult.FullName?.Trim();

                    if (!string.IsNullOrWhiteSpace(playerName))
                    {
                        if (!playersCache.TryGetValue(playerName, out bdPlayer))
                        {
                            bdPlayer =  nsContext.Players
                                .FirstOrDefault(p => p.Name == playerName);

                            if (bdPlayer == null)
                            {
                                bdPlayer = new Player
                                {
                                    Title = playerName,
                                    Name = playerName,
                                    Gender = (GamesResults.Utils.Gender)playerResult.Gender,
                                    DistrictId = bdDistrict?.Id,
                                    District = bdDistrict,
                                    CreatedAt = DateTime.UtcNow
                                };
                                nsContext.Players.Add(bdPlayer);
                            }

                            playersCache[playerName] = bdPlayer;
                        }
                    }
                    await nsContext.SaveChangesAsync();
                    if (bdPlayer != null)
                    {
                        BaseTournamentResult participation=null;

                        // Проверяем, является ли результат командным
                        if (playerResult.IsTeam && !string.IsNullOrWhiteSpace(playerResult.TeamName))
                        {
                            // ========== ОБРАБОТКА КОМАНД ==========

                            // 1. Собираем всех участников команды из всех таблиц
                            var allTeamMembers = new List<PlayerDto>();

                            // Добавляем капитана
                            allTeamMembers.Add(new PlayerDto
                            {
                                Name = playerResult.FullName,
                                // Сохраняем данные капитана
                                GameScores = new[]
                                {
                                    playerResult.Game1,
                                    playerResult.Game2,
                                    playerResult.Game3,
                                    playerResult.Game4,
                                    playerResult.Game5,
                                    playerResult.Game6
                                },
                                Total = playerResult.Total,
                                Average = playerResult.Average
                            });

                            // Добавляем остальных участников из TeamMembers
                            for (int i = 0; i < playerResult.TeamMembers.Count; i++)
                            {
                                var memberName = playerResult.TeamMembers[i];

                                // Ищем результаты этого участника во всех таблицах
                                var memberResult = allPlayers.FirstOrDefault(p =>
                                    p.FullName?.Trim() == memberName?.Trim());

                                if (memberResult != null)
                                {
                                    allTeamMembers.Add(new PlayerDto
                                    {
                                        Name = memberName,
                                        GameScores = new[]
                                        {
                                            memberResult.Game1,
                                            memberResult.Game2,
                                            memberResult.Game3,
                                            memberResult.Game4,
                                            memberResult.Game5,
                                            memberResult.Game6
                                        },
                                        Total = memberResult.Total,
                                        Average = memberResult.Average
                                    });
                                }
                            }

                            // 2. Поиск или создание команды
                            var team = await nsContext.Teams
                                .FirstOrDefaultAsync(t =>
                                    t.Name == playerResult.TeamName &&
                                    t.TournamentId == turnir.Id);

                            if (team == null)
                            {
                                team = new Team
                                {
                                    Name = playerResult.TeamName,
                                    Abbreviation = GetTeamAbbreviation(playerResult.TeamName),
                                    TournamentId = turnir.Id,
                                    CreatedAt = DateTime.UtcNow
                                };
                                nsContext.Teams.Add(team);
                                await nsContext.SaveChangesAsync();
                            }

                            // 3. Добавление всех участников команды
                            var teamMembersProcessed = new HashSet<long>();

                            foreach (var memberData in allTeamMembers)
                            {
                                // Поиск или создание игрока
                                var memberPlayer = await FindOrCreatePlayerAsync(
                                    memberData.Name,
                                    nsContext,
                                    districtsCache,
                                    playersCache);

                                if (memberPlayer == null || teamMembersProcessed.Contains(memberPlayer.Id))
                                    continue;
                                // Если это новый игрок, сохраняем его сразу
                                if (memberPlayer.Id == 0)
                                {
                                    await nsContext.SaveChangesAsync();
                                }
                                // Проверяем, не добавлен ли уже этот игрок в команду
                                var existingMember = await nsContext.TeamMembers
                                    .FirstOrDefaultAsync(tm => tm.TeamId == team.Id && tm.PlayerId == memberPlayer.Id);

                                if (existingMember == null)
                                {
                                    var teamMember = new TeamMember
                                    {
                                        TeamId = team.Id,
                                        PlayerId = memberPlayer.Id,
                                        IsCaptain = (memberData.Name == playerResult.FullName),
                                        Role = (memberData.Name == playerResult.FullName) ?
                                            TeamMemberRole.Captain : TeamMemberRole.Member,
                                        OrderNumber = teamMembersProcessed.Count + 1,
                                        JoinedDate = DateTime.UtcNow,
                                        CreatedAt = DateTime.UtcNow
                                    };
                                    nsContext.TeamMembers.Add(teamMember);
                                }

                                teamMembersProcessed.Add(memberPlayer.Id);
                            }

                            await nsContext.SaveChangesAsync();

                            // 4. Проверяем, существует ли уже результат для этой команды
                            var existingTeamResult = await nsContext.TeamResults
                                .FirstOrDefaultAsync(tr =>
                                    tr.TournamentId == turnir.Id &&
                                    tr.TeamId == team.Id);

                            if (existingTeamResult != null)
                            {
                                continue; // Результат уже существует, пропускаем
                            }

                            // 5. АГРЕГАЦИЯ РЕЗУЛЬТАТОВ КОМАНДЫ
                            // Собираем все результаты участников этой команды
                            var allTeamResults = new List<PlayerResult>();

                            // Ищем капитана
                            var captainResult = allPlayers.FirstOrDefault(p =>
                                p.FullName?.Trim() == playerResult.FullName?.Trim() &&
                                p.TeamName?.Trim() == playerResult.TeamName?.Trim());

                            if (captainResult != null)
                            {
                                allTeamResults.Add(captainResult);
                            }

                            // Ищем остальных участников команды
                            foreach (var memberName in playerResult.TeamMembers)
                            {
                                var memberResult = allPlayers.FirstOrDefault(p =>
                                    p.FullName?.Trim() == memberName?.Trim() &&
                                    p.TeamName?.Trim() == playerResult.TeamName?.Trim());

                                if (memberResult != null)
                                {
                                    allTeamResults.Add(memberResult);
                                }
                            }

                            // Если не нашли участников по TeamName, ищем просто по имени
                            if (!allTeamResults.Any())
                            {
                                allTeamResults = allPlayers
                                    .Where(p => p.TeamName?.Trim() == playerResult.TeamName?.Trim())
                                    .ToList();
                            }

                            // 6. Расчет командных показателей на основе ВСЕХ участников
                            decimal teamTotal = 0;
                            decimal teamAverage = 0;
                            int totalGamesPlayed = 0;
                            var allGameScores = new List<int>();
                            var memberScores = new Dictionary<long, int>();

                            foreach (var teamMemberResult in allTeamResults)
                            {
                                // Находим игрока в БД
                                var teamPlayer = await FindOrCreatePlayerAsync(
                                    teamMemberResult.FullName,
                                    nsContext,
                                    districtsCache,
                                    playersCache);

                                if (teamPlayer == null) continue;

                                // Добавляем баллы игрока
                                memberScores[teamPlayer.Id] = teamMemberResult.Total;

                                // Суммируем общие показатели
                                teamTotal += teamMemberResult.Total;
                                teamAverage += (decimal)teamMemberResult.Average;
                                totalGamesPlayed += teamMemberResult.PlayedGamesCount;

                                // Собираем все игровые очки
                                var memberGameScores = new[]
                                {
                                    teamMemberResult.Game1,
                                    teamMemberResult.Game2,
                                    teamMemberResult.Game3,
                                    teamMemberResult.Game4,
                                    teamMemberResult.Game5,
                                    teamMemberResult.Game6
                                };
                                allGameScores.AddRange(memberGameScores.Where(score => score > 0));
                            }

                            // Рассчитываем средние значения
                            if (allTeamResults.Any())
                            {
                                teamAverage = teamAverage / allTeamResults.Count;
                            }

                            // 7. Создание КОМАНДНОГО результата
                            var teamResult = new TeamResult
                            {
                                TournamentId = turnir.Id,
                                TeamId = team.Id,
                                Place = playerResult.Place, // Используем место из данных капитана
                                TotalScore = (int)teamTotal,
                                AverageScore = teamAverage,
                                GamesPlayed = totalGamesPlayed,
                                ResultDate = DateTime.UtcNow,
                                Notes = $"Команда: {playerResult.TeamName}, Участников: {allTeamResults.Count}"
                            };

                            // Сохраняем агрегированные результаты игр как JSON
                            // Можно сохранить средние значения по играм или все очки
                            var averageGameScores = new List<decimal>();
                            for (int i = 0; i < 6; i++)
                            {
                                var gameScores = allTeamResults
                                    .Where(r => r.GetGameScore(i) > 0)
                                    .Select(r => r.GetGameScore(i))
                                    .ToList();

                                if (gameScores.Any())
                                {
                                    averageGameScores.Add((decimal)gameScores.Average());
                                }
                                else
                                {
                                    averageGameScores.Add(0);
                                }
                            }

                            teamResult.GameScoresJson = JsonSerializer.Serialize(averageGameScores);
                            teamResult.MemberScoresJson = JsonSerializer.Serialize(memberScores);

                            participation = teamResult;
                        }
                        else
                        {
                            // ========== ИНДИВИДУАЛЬНЫЙ РЕЗУЛЬТАТ ==========

                            var individualResult = new IndividualResult
                            {
                                TournamentId = turnir.Id,
                                PlayerId = bdPlayer.Id,
                                Place = playerResult.Place,
                                TotalScore = playerResult.Total,
                                AverageScore = (decimal)playerResult.Average,
                                GamesPlayed = playerResult.PlayedGamesCount,
                                ResultDate = DateTime.UtcNow,
                                Notes = $"Индивидуальное участие"
                            };

                            // Сохраняем результаты игр как JSON
                            var gameScores = new[]
                            {
                                playerResult.Game1,
                                playerResult.Game2,
                                playerResult.Game3,
                                playerResult.Game4,
                                playerResult.Game5,
                                playerResult.Game6
                            };
                            individualResult.GameScoresJson = JsonSerializer.Serialize(gameScores);

                            // Дополнительная статистика
                            individualResult.HighGame = playerResult.BestGameScore;
                            individualResult.LowGame = playerResult.WorstGameScore;
                            individualResult.StrikeCount = 0; // Можно парсить из PDF, если есть данные
                            individualResult.SpareCount = 0;  // Можно парсить из PDF, если есть данные

                            participation = individualResult;
                        }

                        if (participation != null)
                        {
                            tableResults.Add(participation);
                        }

                        tableResults.Add(participation);
                    }
                }

                // 11. Сохранение всех Participation
                if (tableResults.Any())
                {
                    nsContext.TournamentResults.AddRange(tableResults);
                }

                // 12. Сохранение всех изменений
                await nsContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 13. Возврат результата
                return Ok(new
                {
                    Success = true,
                    Message = "PDF и данные успешно загружены",
                    TournamentId = turnir.Id,
                    DocumentId = TournamentDocument.Id, // Добавляем ID документа
                    Tournament = turnir.Name,
                    Year = turnir.StartDate?.Year,
                    City = city.Title,
                    PlayersCount = tableResults.Count,
                    FileName = TournamentDocument.OriginalFileName,
                    FileSize = TournamentDocument.FileSize,
                    CreatedAt = TournamentDocument.CreatedAt
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    Title = "Ошибка базы данных",
                    Detail = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                //_logger.LogError(ex, "Ошибка при загрузке PDF с данными");
                return StatusCode(500, new
                {
                    Title = "Внутренняя ошибка сервера",
                    Detail = ex.Message
                });
            }
            
        }
        // Вспомогательный метод для поиска/создания игрока
        private async Task<Player> FindOrCreatePlayerAsync(
            string playerName,
            AppDbContext context,
            Dictionary<string, District> districtsCache,
            Dictionary<string, Player> playersCache)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return null;

            var name = playerName.Trim();

            // Проверяем кэш
            if (playersCache.TryGetValue(name, out var cachedPlayer))
                return cachedPlayer;

            // Ищем в БД
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Name == name);

            if (player == null)
            {
                // Создаем нового игрока
                player = new Player
                {
                    Title = name,
                    Name = name,
                    Gender = Gender.Unknown, // Не знаем пол для участников команды
                    CreatedAt = DateTime.UtcNow
                };

                // Можно попробовать определить район, если есть в каких-то данных
                context.Players.Add(player);
            }

            // Сохраняем в кэш
            playersCache[name] = player;

            return player;
        }

        // Вспомогательный метод для создания аббревиатуры команды
        private string GetTeamAbbreviation(string teamName)
        {
            if (string.IsNullOrWhiteSpace(teamName))
                return string.Empty;

            // Простая логика: берем первые буквы слов
            var words = teamName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
                return teamName.Length > 3 ? teamName.Substring(0, 3).ToUpper() : teamName.ToUpper();

            return new string(words.Select(w => w[0]).ToArray()).ToUpper();
        }

        // GET: api/Tournaments/{TournamentId}/documents
        [HttpGet("/documents")]
        public IActionResult GetReportStream(long TournamentId)
        {
            var document =  nsContext.TournamentDocuments
         .FirstOrDefault(d => d.TournamentId == TournamentId);

            if (document == null)
                return NotFound();

            byte[] fileData;

            
            // Читаем из bytea
            fileData = document.FileData ?? Array.Empty<byte>();
            

            if (fileData == null || fileData.Length == 0)
                return NotFound("File data is empty");

            Console.WriteLine($"Downloading document {TournamentId}: {fileData.Length} bytes");

            return File(
                fileData,
                document.ContentType ?? "application/octet-stream",
                document.OriginalFileName);
        }
    }
    public static class PlayerResultExtensions
    {
        public static int GetGameScore(this PlayerResult playerResult, int gameIndex)
        {
            return gameIndex switch
            {
                0 => playerResult.Game1,
                1 => playerResult.Game2,
                2 => playerResult.Game3,
                3 => playerResult.Game4,
                4 => playerResult.Game5,
                5 => playerResult.Game6,
                _ => 0
            };
        }
        
    }
    public class PlayerDto
    {
        public string Name { get; set; }
        public int[] GameScores { get; set; }
        public int Total { get; set; }
        public double Average { get; set; }
    }
}
