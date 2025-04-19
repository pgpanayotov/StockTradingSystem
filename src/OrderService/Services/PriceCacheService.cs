using System.Collections.Concurrent;
using MassTransit;
using Shared.Contracts;

namespace OrderService.Services
{
    // Redis might be suitable for a distributed centralized cache, but for simplicity, we are using an in-memory cache.
    public class PriceCacheService : IConsumer<PriceUpdated>
    {
        private readonly ConcurrentDictionary<string, decimal> _prices = new();

        public decimal? GetPrice(string ticker) =>
            _prices.TryGetValue(ticker.ToUpper(), out var price) ? price : null;

        public Task Consume(ConsumeContext<PriceUpdated> context)
        {
            _prices[context.Message.Ticker.ToUpper()] = context.Message.Price;
            return Task.CompletedTask;
        }
    }
}
