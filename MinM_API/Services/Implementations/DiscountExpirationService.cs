using Microsoft.EntityFrameworkCore;
using MinM_API.Data;

namespace MinM_API.Services.Implementations
{
    public class DiscountExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DiscountExpirationService> _logger;

        public DiscountExpirationService(IServiceScopeFactory scopeFactory, ILogger<DiscountExpirationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation("Наступна перевірка знижок через {Delay}", delay);

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var expiredDiscounts = await db.Discounts
                        .Where(d => d.IsActive && d.EndDate < DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    foreach (var discount in expiredDiscounts)
                    {
                        if (discount.RemoveAfterExpiration)
                        {
                            db.Discounts.Remove(discount);
                            _logger.LogInformation("Знижку {Name} видалено", discount.Name);
                        }
                        else
                        {
                            discount.IsActive = false;
                            _logger.LogInformation("Знижку {Name} деактивовано", discount.Name);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка при обробці завершених знижок");
                }
            }
        }
    }

}
