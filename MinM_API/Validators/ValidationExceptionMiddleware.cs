using MinM_API.Extension;

namespace MinM_API.Validators
{
    public class ValidationExceptionMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (DtoValidationException exception)
            {
                context.Response.StatusCode = (int)exception.StatusCode;
                await context.Response.WriteAsJsonAsync(
                    ResponseFactory.Error<object?>(null, exception.Message, exception.StatusCode));
            }
        }
    }
}
