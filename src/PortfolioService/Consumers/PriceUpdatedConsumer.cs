using MassTransit;
using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using Shared.Contracts;

namespace PortfolioService.Consumers
{
    public class PriceUpdatedConsumer : IConsumer<PriceUpdated>
    {
        private readonly PortfolioDbContext _db;

        public PriceUpdatedConsumer(PortfolioDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<PriceUpdated> context)
        {
            // This is implemented according to the requirements of the task to update the price when event arrives.
            // However my personal opinion is that updating the values constantly is a bit overkill and adding too much load on the database.
            // Perhaps the prices can be cached in Redis and consume them from centralized place when needed ?
            var update = context.Message;

            var items = await _db.PortfolioItems
                .Where(i => i.Ticker == update.Ticker)
                .ToListAsync();

            foreach (var item in items)
            {
                item.LatestPrice = update.Price;
            }

            await _db.SaveChangesAsync();
        }
    }
}
