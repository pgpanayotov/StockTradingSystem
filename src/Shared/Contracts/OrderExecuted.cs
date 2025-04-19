namespace Shared.Contracts;

public class OrderExecuted
{
    public string UserId { get; set; } = default!;
    public string Ticker { get; set; } = default!;
    public int Quantity { get; set; }
    public string Side { get; set; } = default!;
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}