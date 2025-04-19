using Shared.Contracts;
using MassTransit;

namespace PriceService.Services;

public class PriceGeneratorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string[] _tickers = new[] { "AAPL", "TSLA", "NVDA", "GOOG", "AMZN" };
    private readonly Random _random = new();

    public PriceGeneratorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            foreach (var ticker in _tickers)
            {
                var price = Math.Round((decimal)(_random.NextDouble() * 1000), 2);

                var message = new PriceUpdated
                {
                    Ticker = ticker,
                    Price = price,
                    Timestamp = DateTime.UtcNow
                };

                await publisher.Publish(message, stoppingToken);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
