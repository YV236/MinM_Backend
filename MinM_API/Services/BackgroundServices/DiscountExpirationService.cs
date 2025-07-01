using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using System.Linq;

namespace MinM_API.Services.BackgroundServices
{
    public class DiscountExpirationService(IServiceScopeFactory scopeFactory, ILogger<DiscountExpirationService> logger) : BackgroundService
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
                logger.LogInformation("Наступна перевірка знижок через {Delay}", delay);

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var expiredDiscounts = await db.Discounts
                        .Where(d => d.IsActive && d.EndDate < DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    foreach (var discount in expiredDiscounts)
                    {
                        foreach (var product in discount.Products.ToList())
                        {
                            product.Discount = null;
                            product.IsDiscounted = false;

                            foreach (var productVariant in product.ProductVariants)
                            {
                                productVariant.DiscountPrice = null;
                            }
                        }

                        if (discount.RemoveAfterExpiration)
                        {
                            db.Discounts.Remove(discount);
                            logger.LogInformation("Знижку {Name} видалено", discount.Name);
                        }
                        else
                        {
                            discount.IsActive = false;
                            logger.LogInformation("Знижку {Name} деактивовано", discount.Name);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Помилка при обробці завершених знижок");
                }
            }
        }
    }

}
