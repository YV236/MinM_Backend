using Microsoft.EntityFrameworkCore;
using MinM_API.Data;

namespace MinM_API.Services.BackgroundServices
{
    public class SeasonExpirationService(IServiceScopeFactory scopeFactory, ILogger<SeasonExpirationService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                logger.LogInformation("Наступна перевірка сезонів через {Delay}", delay);

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var expiredSeasons = await db.Seasons
                        .Where(d => d.EndDate < DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    foreach (var season in expiredSeasons)
                    {
                        foreach (var product in season.Products.ToList())
                        {
                            product.Season = null;
                            product.IsSeasonal = false;
                        }

                        db.Seasons.Remove(season);
                        logger.LogInformation("Сезон {Name} видалено", season.Name);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Помилка при обробці завершених сезонів");
                }
            }
        }
    }
}
