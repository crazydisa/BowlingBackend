using GamesResults.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace GamesResults
{
    // Background service
    public class RatingUpdateService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<RatingUpdateService> _logger;

        public RatingUpdateService(IServiceProvider services, ILogger<RatingUpdateService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rating Update Service запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Находим турниры, завершившиеся за последние 24 часа
                    var recentTournaments = await context.Tournaments
                        .Where(t => t.EndDate >= DateTime.UtcNow.AddDays(-1) &&
                                    t.EndDate < DateTime.UtcNow &&
                                    !t.RatingsUpdated)
                        .ToListAsync();

                    foreach (var tournament in recentTournaments)
                    {
                        await ratingService.UpdateRatingsAfterTournamentAsync(tournament.Id);
                        tournament.RatingsUpdated = true;
                        await context.SaveChangesAsync();

                        _logger.LogInformation("Рейтинги обновлены для турнира {TournamentName}", tournament.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении рейтингов");
                }

                // Ждем 1 час до следующей проверки
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
