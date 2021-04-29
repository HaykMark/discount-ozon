using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using static Microsoft.AspNetCore.Http.HttpMethods;

namespace Discounting.API.Common.Extensions
{
    /// <summary>
    /// Helper class to deal with action filter contexts
    /// </summary>
    public static class ActionFilterContextHelper
    {
        public static bool IsUpdateOrCreateOperation(this ActionExecutingContext context)
        {
            var httpMethod = context.HttpContext.Request.Method;
            return IsPost(httpMethod) || IsPut(httpMethod) || IsPatch(httpMethod);
        }

        public static bool IsGetOperation(this ActionExecutingContext context)
        {
            var httpMethod = context.HttpContext.Request.Method;
            return IsGet(httpMethod);
        }

        /// <summary>
        /// Returns the payload that is associated with the `[FromBody]` attribute
        /// or null if JSON.NET failed parsing the body.
        /// </summary>
        public static object GetDto(this ActionExecutingContext context)
        {
            var parameters = context.ActionDescriptor.Parameters;
            return (
                from ControllerParameterDescriptor parameter in parameters
                let attributes = parameter.ParameterInfo.CustomAttributes
                where
                    attributes.Any(a =>
                        (a.AttributeType == typeof(FromBodyAttribute))
                        && context.ActionArguments.ContainsKey(parameter.Name))
                select context.ActionArguments[parameter.Name]
            ).FirstOrDefault();
        }
    }
}