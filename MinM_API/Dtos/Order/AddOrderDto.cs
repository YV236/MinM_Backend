namespace MinM_API.Dtos.Order
{
    public class AddOrderDto
    {
        public string AddressId { get; set; } = Guid.NewGuid().ToString();
        public Address Address { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = string.Empty;

        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
    }
}
