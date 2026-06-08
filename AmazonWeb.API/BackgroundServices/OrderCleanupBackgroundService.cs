using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AmazonWeb.API.BackgroundServices
{
    public class OrderCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderCleanupBackgroundService> _logger;
        private readonly TimeSpan _executionInterval = TimeSpan.FromMinutes(5); // Wakes up every 5 minutes

        public OrderCleanupBackgroundService(IServiceProvider serviceProvider, ILogger<OrderCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order Cleanup Background Service is starting up.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Executing scheduled pending orders expiration routine...");

                    // DbContext and Repositories are Scoped; BackgroundService is Singleton. 
                    // We must create an explicit scope here to safely instantiate the Repository.
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                        // Calculate the target cutoff timestamp (15 minutes ago)
                        DateTime cutoffTime = DateTime.UtcNow.AddMinutes(-15);

                        int deletedCount = await orderRepository.DeleteExpiredPendingOrdersAsync(cutoffTime);

                        if (deletedCount > 0)
                        {
                            _logger.LogInformation("Successfully cleared out {Count} expired pending orders.", deletedCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the pending order cleanup background task.");
                }

                // Sleep comfortably for 5 minutes before checking again
                await Task.Delay(_executionInterval, stoppingToken);
            }

            _logger.LogInformation("Order Cleanup Background Service is shutting down.");
        }
    }
}