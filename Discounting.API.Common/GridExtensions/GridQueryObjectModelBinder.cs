using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Discounting.API.Common.GridExtensions
{
    public class GridQueryObjectModelBinder<T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            string queryObject;
            if (bindingContext.HttpContext.Request.Method == "GET")
            {
                //Check and get source data from query string. 
                if (!bindingContext.HttpContext.Request.QueryString.HasValue)
                {
                    throw new ArgumentException("queryObject as QueryString is not provided");
                }
                queryObject = bindingContext.ActionContext.HttpContext.Request.Query["queryObject"];
            }
            else
            {
                queryObject = new StreamReader(bindingContext.ActionContext.HttpContext.Request.Body).ReadToEnd();
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(queryObject);
                //Assign completed object tree to Model and return it.
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
            }

            return Task.CompletedTask;
        }
    }
}