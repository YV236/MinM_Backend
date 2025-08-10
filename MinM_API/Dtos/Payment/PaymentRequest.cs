namespace MinM_API.Dtos.Payment
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string OrderId { get; set; }
        public string CallbackUrl { get; set; }
        public string ResultUrl { get; set; }

        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
    }

}
