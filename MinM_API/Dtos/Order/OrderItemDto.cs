using System.Globalization;

namespace MinM_API.Dtos.Order
{
    public class OrderItemDto
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}