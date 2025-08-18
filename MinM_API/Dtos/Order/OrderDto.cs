using MinM_API.Dtos.User;

namespace MinM_API.Dtos.Order
{
    public class OrderDto
    {
        public string Id { get; set; }
        public string AddressId { get; set; }
        public Address Address { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = string.Empty;

        public string RecipientFirstName { get; set; } = string.Empty;
        public string RecipientLastName { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientPhone { get; set; }
    }
}
