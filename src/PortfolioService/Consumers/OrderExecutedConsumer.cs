using MassTransit;
using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using PortfolioService.Models;
using Shared.Contracts;

namespace PortfolioService.Consumers
{
    public class OrderExecutedConsumer : IConsumer<OrderExecuted>
    {
        private readonly PortfolioDbContext _db;

        public OrderExecutedConsumer(PortfolioDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<OrderExecuted> context)
        {
            var order = context.Message;

            var portfolio = await _db.Portfolios
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.UserId == order.UserId)
                ?? new Portfolio { UserId = order.UserId };

            var item = portfolio.Items.FirstOrDefault(i => i.Ticker == order.Ticker);
            if (item == null)
            {
                item = new PortfolioItem { Ticker = order.Ticker, Quantity = 0 };
                portfolio.Items.Add(item);
            }

            // If quantity is 0 and we receive sell execution it is most probably invalid scenario.
            // It depends on the business logic if we allow negative values here.
            // If not we need to think about providing the service that raises this with details about the quantity that is available for selling.
            // Leaving it as is for now, since it is not specified in the task.
            item.Quantity += order.Side.ToLower() == "buy" ? order.Quantity : -order.Quantity;
            item.LatestPrice = order.Price;

            _db.Update(portfolio);
            await _db.SaveChangesAsync();
        }
    }
}
