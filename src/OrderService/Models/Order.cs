namespace OrderService.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = default!;
        public string Ticker { get; set; } = default!;
        public int Quantity { get; set; }
        public string Side { get; set; } = default!;
        public decimal ExecutedPrice { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
