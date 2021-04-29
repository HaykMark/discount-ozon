using System;
using System.Linq;
using System.Text.RegularExpressions;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Extensions;
using Discounting.Common.Exceptions;
using Discounting.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Discounting.API.Common.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Checks for id mismatches between id included via PUT in
    /// url and the id included via payload.
    /// </summary>
    public class RouteValidatorActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            if (request.Method != HttpMethods.Put ||
                context.Filters.OfType<DisableRoutValidator>().Any())
            {
                // irrelevant
                return;
            }

            var urlId = new Regex(@"^\/api\/.+\/([A-Za-z0-9]+)$")
                .Match(request.Path.Value)
                .Groups[1]
                .Value;

            var dto = context.GetDto();
            if (dto == null)
            {
                return;
            }

            var idPropertyKey = RouteParams.Id.Capitalize();
            var propInfo = dto.GetType().GetProperty(idPropertyKey);
            var dtoId = propInfo?.GetValue(dto).ToString();

            if (urlId != "" && dtoId != null && urlId != dtoId)
            {
                throw new RouteArgumentMismatchException(idPropertyKey, dtoId, urlId);
            }
        }

        /// <inheritdoc />
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // ignore; just exists to satisfy the interface
        }
    }

    /// <summary>
    /// Deactivate RouteValidatorActionFilter
    /// </summary>
    public class DisableRoutValidator : Attribute, IFilterMetadata
    {
    }
}