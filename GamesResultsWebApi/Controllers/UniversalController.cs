
using GamesResults.Models.Bowling;
using GamesResults.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfTableReader2;
using System;
using System.Globalization;

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
                var tableResults = new List<TournamentResult>();
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
                                    CreatedAt = DateTime.UtcNow
                                };
                                nsContext.Districts.Add(bdDistrict);
                            }

                            districtsCache[regionName] = bdDistrict;
                        }
                    }

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

                    if (bdPlayer != null)
                    {
                        var participation = new TournamentResult
                        {
                            Title = bdPlayer.Name,
                            PlayerId = bdPlayer.Id,
                            Player = bdPlayer,
                            
                            TournamentId = turnir.Id,
                            Tournament = turnir,
                            Game1 = playerResult.Game1,
                            Game2 = playerResult.Game2,
                            Game3 = playerResult.Game3,
                            Game4 = playerResult.Game4,
                            Game5 = playerResult.Game5,
                            Game6 = playerResult.Game6,
                            CreatedAt = DateTime.UtcNow
                        };
                        tableResults.Add(participation);
                    }
                }

                // 11. Сохранение всех Participation
                if (tableResults.Any())
                {
                    nsContext.Results.AddRange(tableResults);
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

}
