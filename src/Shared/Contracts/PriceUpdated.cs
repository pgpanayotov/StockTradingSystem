namespace Shared.Contracts;

public class PriceUpdated
{
    public string Ticker { get; set; } = default!;
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}
