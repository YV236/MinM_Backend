namespace MinM_API.Dtos.Payment
{
    public class PaymentCallbackResponse
    {
        public string Status { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
