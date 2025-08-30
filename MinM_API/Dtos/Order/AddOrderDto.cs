using MinM_API.Dtos.Address;

namespace MinM_API.Dtos.Order
{
    public class AddOrderDto
    {
        public AddressDto Address { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = string.Empty;

        public string RecipientFirstName { get; set; } = string.Empty;
        public string RecipientLastName { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientPhone { get; set; }
    }
}
