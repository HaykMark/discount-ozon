using Discounting.API.Common.Response;
using Discounting.API.Common.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Discounting.API.Common.Filters
{
   /// <summary>
    /// This action filter will wrap a successful response in a
    /// propper data response object (in case this is not the case yet).
    /// </summary>
    /// <remarks>
    /// If the result returned by the controller action is empty then
    /// this filter will set the value to NotFoundResult.
    /// </remarks>
    public class ResponsePostProcessor : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // ignore; just exists to satisfy the IActionFilter interface
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result;
            if (!(result is JsonResult || result is ObjectResult))
            {
                // might be something else, e.g. a PhysicalFileResult,
                // so we simply ignore such cases
                return;
            }

            var propInfo = result.GetType().GetProperty("Value");
            var value = propInfo?.GetValue(result);

            if (value == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            if (!(value is ResponseBody))
            {
                value = new DataResponseBody<object>(value);
            }

            var dataResponseBody = value as DataResponseBody<object>;
            ConstructMetaData(dataResponseBody);
            value = dataResponseBody;

            propInfo.SetValue(context.Result, value);
        }

        private void ConstructMetaData<T>(DataResponseBody<T> dataResponseBody)
        {
            //If we get more types we should use switch
            if (dataResponseBody.data is SessionInfoDTO)
            {
                var responseBody = dataResponseBody as ResponseBody;
            }
        }
    }
}