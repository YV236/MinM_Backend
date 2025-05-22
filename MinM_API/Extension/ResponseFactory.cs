using MinM_API.Dtos;
using System.Net;

namespace MinM_API.Extension
{
    public static class ResponseFactory
    {
        public static ServiceResponse<T> Error<T>(T data, string message, HttpStatusCode code = HttpStatusCode.InternalServerError)
            => new() { Data = data, IsSuccessful = false, Message = message, StatusCode = code };

        public static ServiceResponse<T> Success<T>(T data, string message = "", HttpStatusCode code = HttpStatusCode.OK)
            => new() { Data = data, IsSuccessful = true, Message = message, StatusCode = code };
    }
}
