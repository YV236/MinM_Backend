using System.Net;

namespace MinM_API.Validators
{
    public class DtoValidationException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : Exception(message)
    {
        public HttpStatusCode StatusCode { get; } = statusCode;
    }
}
