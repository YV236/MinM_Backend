using System.Globalization;

namespace MinM_API.Dtos.Order
{
    public class OrderItemDto
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ColorId { get; set; }
        // Response snapshots; supplied display values are ignored when creating an order.
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
    }
}
