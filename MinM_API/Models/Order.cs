namespace MinM_API.Models
{
    public class Order
    {
        public string Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; } = "guest";
        public virtual User? User { get; set; } = null;
        public string AddressId { get; set; }
        public virtual Address Address { get; set; } = null!;
        public virtual List<OrderItem> OrderItems { get; set; } = [];
        public Status Status { get; set; }
        public string PaymentMethod { get; set; } = "Card";
        public string DeliveryMethod { get; set; } = "NovaPost";
        public long OrderNumber { get; set; }

        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
    }
}
