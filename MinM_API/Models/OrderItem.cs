namespace MinM_API.Models
{
    public class OrderItem
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        public string ItemId { get; set; }
        public virtual ProductVariant Item { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
