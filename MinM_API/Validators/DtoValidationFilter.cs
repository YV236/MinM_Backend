using Microsoft.AspNetCore.Mvc.Filters;

namespace MinM_API.Validators
{
    public class DtoValidationFilter(IEnumerable<IDtoValidator> validators) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values.Where(value => value is not null))
            {
                foreach (var validator in validators.Where(v => v.DtoType.IsInstanceOfType(argument)))
                {
                    await validator.ValidateAsync(argument!, context.HttpContext.RequestAborted);
                }
            }

            await next();
        }
    }
}
