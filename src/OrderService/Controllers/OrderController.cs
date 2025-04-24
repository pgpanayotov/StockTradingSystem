using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Services;
using Shared.Contracts;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        // This is done in the controller for simplicity.
        // In a real-world application we would like to use a service layer or mediator to
        // decouple the logic from the controllers and have only one dependency and keep the controllers clean from business logic.
        private readonly OrderDbContext _db;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly PriceCacheService _priceCache;

        public OrderController(OrderDbContext db, IPublishEndpoint publishEndpoint, PriceCacheService priceCache)
        {
            _db = db;
            _publishEndpoint = publishEndpoint;
            _priceCache = priceCache;
        }

        [HttpPost("add/{userId}")]
        public async Task<IActionResult> PlaceOrder(string userId, [FromBody] OrderRequest request)
        {
            var price = _priceCache.GetPrice(request.Ticker);
            if (price == null)
                return BadRequest("Price not available for this ticker yet.");

            var order = new Order
            {
                UserId = userId,
                Ticker = request.Ticker,
                Quantity = request.Quantity,
                Side = request.Side,
                ExecutedPrice = price.Value,
                Timestamp = DateTime.UtcNow
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            await _publishEndpoint.Publish(new OrderExecuted
            {
                UserId = userId,
                Ticker = request.Ticker,
                Quantity = request.Quantity,
                Side = request.Side,
                Price = price.Value,
                Timestamp = order.Timestamp
            });

            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _db.Orders.AsNoTracking().ToListAsync();
            return Ok(orders);
        }
    }

    public class OrderRequest
    {
        [Required(ErrorMessage = "Ticker is required.")]
        public string Ticker { get; set; } = default!;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Side is required.")]
        public string Side { get; set; } = default!;
    }
}
