using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;

namespace PortfolioService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioDbContext _db;

        public PortfolioController(PortfolioDbContext db)
        {
            _db = db;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetPortfolio(string userId)
        {
            var portfolio = await _db.Portfolios
                .Include(p => p.Items)
                .AsNoTrackingWithIdentityResolution()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (portfolio == null) return NotFound();

            return Ok(new
            {
                UserId = portfolio.UserId,
                TotalValue = portfolio.TotalValue
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPortfolios()
        {
            var portfolios = await _db.Portfolios
                .Include(p => p.Items)
                .AsNoTrackingWithIdentityResolution()
                .ToListAsync();

            return Ok(portfolios);
        }
    }
}
