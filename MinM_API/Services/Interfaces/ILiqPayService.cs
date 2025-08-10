using MinM_API.Dtos.Payment;

namespace MinM_API.Services.Interfaces
{

    public interface ILiqPayService
    {
        string CreatePaymentForm(PaymentRequest request);
        Task<PaymentCallbackResponse> ProcessCallback(string data, string signature);
    }
}
