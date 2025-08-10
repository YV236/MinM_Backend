using MinM_API.Dtos.Payment;
using MinM_API.Services.Interfaces;

namespace MinM_API.Services.Implementations
{
    public class LiqPayService : ILiqPayService
    {
        private readonly LiqPay _liqPay;
        private readonly IConfiguration _configuration;

        public LiqPayService(IConfiguration configuration)
        {
            _configuration = configuration;
            var publicKey = configuration["LiqPay:PublicKey"];
            var privateKey = configuration["LiqPay:PrivateKey"];
            _liqPay = new LiqPay(publicKey, privateKey);
        }

        public string CreatePaymentForm(PaymentRequest request)
        {
            var parameters = new Dictionary<string, object>
        {
            { "action", "pay" },
            { "amount", request.Amount },
            { "currency", "UAH" },
            { "description", request.Description },
            { "order_id", request.OrderId },
            { "version", "3" },
            { "server_url", request.CallbackUrl },
            { "result_url", request.ResultUrl }
        };

            return _liqPay.CNBForm(parameters);
        }

        public async Task<PaymentCallbackResponse> ProcessCallback(string data, string signature)
        {
            // Перевірка підпису
            var isValidSignature = _liqPay.StrToSign(data) == signature;

            if (!isValidSignature)
            {
                throw new UnauthorizedAccessException("Invalid signature");
            }

            // Декодування даних
            var decodedData = _liqPay.DecodeParams(data);

            return new PaymentCallbackResponse
            {
                Status = decodedData["status"].ToString(),
                OrderId = decodedData["order_id"].ToString(),
                Amount = decimal.Parse(decodedData["amount"].ToString()),
                Currency = decodedData["currency"].ToString()
            };
        }
    }

}
