using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinM_API.Data;
using Microsoft.EntityFrameworkCore;

namespace MinM_API.Services.BackgroundServices
{
    public class CheckProductFreshnessService(IServiceProvider scopeFactory, ILogger<CheckProductFreshnessService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();

                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var thresholdDate = DateTime.UtcNow.AddDays(-90);

                    var productsToUpdate = await dbContext.Products
                        .Where(p => p.IsNew && p.DateOfCreation < thresholdDate)
                        .ToListAsync(stoppingToken);

                    if (productsToUpdate.Count != 0)
                    {
                        foreach (var product in productsToUpdate)
                        {
                            product.IsNew = false;
                        }
                        await dbContext.SaveChangesAsync(stoppingToken);

                        logger.LogInformation("Updated {Count} products' IsNew flag to false.", productsToUpdate.Count);
                    }
                    else
                    {
                        logger.LogInformation("No products needed IsNew flag update.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while checking product freshness.");
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}