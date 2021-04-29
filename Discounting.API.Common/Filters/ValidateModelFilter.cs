using Discounting.API.Common.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Discounting.API.Common.Filters
{
    public class ValidateModelFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // we pass the invalid model state to the validation exception
                // constructor so it can build a serializable validation
                // based on the errors included in the model state.
                // then we throw this exception so it can be cached by a
                // validation middleware layer.
                throw ValidationExceptionExtensions.FromModelState(context.ModelState);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // ignore; just exists to satisfy the interface
        }
    }
}