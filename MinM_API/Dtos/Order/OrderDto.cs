using MinM_API.Dtos.Address;

namespace MinM_API.Dtos.Order
{
    public class OrderDto
    {
        public string Id { get; set; }
        public long OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string? AddressId { get; set; }
        public AddressDto Address { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = [];
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string DeliveryMethod { get; set; }
        public string AdditionalInfo { get; set; }

        public string RecipientFirstName { get; set; }
        public string RecipientLastName { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientPhone { get; set; }
        public string? UserName { get; set; }
    }
}
