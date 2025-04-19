namespace PortfolioService.Models
{
    public class Portfolio
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = default!;
        public ICollection<PortfolioItem> Items { get; set; } = new List<PortfolioItem>();
        public decimal TotalValue => Items.Sum(i => i.Quantity * i.LatestPrice);
    }
}
