namespace PortfolioService.Models
{
    public class PortfolioItem
    {
        public Guid Id { get; set; }
        public string Ticker { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal LatestPrice { get; set; }

        public Portfolio Portfolio { get; set; } = default!;
        public Guid PortfolioId { get; set; } 
    }
}
