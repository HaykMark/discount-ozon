using System.Collections.Generic;
using System.Linq;
using System.Net;
using Discounting.API.Common.Extensions;
using Discounting.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Discounting.API.Common.Filters
{
    public class XssValidatorActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            // Check XSS in URL
            if (!string.IsNullOrWhiteSpace(request.Path.Value))
            {
                var url = request.Path.Value;

                if (CrossSiteScriptingValidation.IsDangerousString(url, out _))
                {
                    throw new ForbiddenException("dangerous-url",
                        "System found dangerous characters in url");
                }
            }

            // Check XSS in query string
            if (!string.IsNullOrWhiteSpace(request.QueryString.Value))
            {
                var queryString = WebUtility.UrlDecode(request.QueryString.Value);

                if (CrossSiteScriptingValidation.IsDangerousString(queryString, out _))
                {
                    throw new ForbiddenException("dangerous-query-param",
                        "System found dangerous characters in query param");
                }
            }

            if (request.Method == HttpMethods.Put && request.Method == HttpMethods.Post)
                return;

            var dto = context.GetDto();
            if (dto == null)
            {
                return;
            }

            var stringProperties = dto.GetType()
                .GetProperties()
                .Where(p => p.PropertyType == typeof(string));

            if (stringProperties.Select(
                    stringProperty => stringProperty.GetValue(dto)?.ToString())
                .Where(content => content != null)
                .Any(content => CrossSiteScriptingValidation.IsDangerousString(content, out _)))
            {
                throw new ForbiddenException("dangerous-content-in-dto",
                    "System found dangerous characters in the request");
            }
        }
    }

    /// <summary>
    /// Imported from System.Web.CrossSiteScriptingValidation Class
    /// </summary>
    public static class CrossSiteScriptingValidation
    {
        private static readonly char[] StartingChars = {'<', '&'};

        #region Public methods

        public static bool IsDangerousString(string s, out int matchIndex)
        {
            //bool inComment = false;
            matchIndex = 0;

            for (var i = 0;;)
            {
                // Look for the start of one of our patterns 
                var n = s.IndexOfAny(StartingChars, i);

                // If not found, the string is safe
                if (n < 0) return false;

                // If it's the last char, it's safe 
                if (n == s.Length - 1) return false;

                matchIndex = n;

                switch (s[n])
                {
                    case '<':
                        // If the < is followed by a letter or '!', it's unsafe (looks like a tag or HTML comment)
                        if (IsAtoZ(s[n + 1]) || s[n + 1] == '!' || s[n + 1] == '/' || s[n + 1] == '?') return true;
                        break;
                    case '&':
                        // If the & is followed by a #, it's unsafe (e.g. S) 
                        if (s[n + 1] == '#') return true;
                        break;
                }

                // Continue searching
                i = n + 1;
            }
        }

        #endregion

        #region Private methods

        private static bool IsAtoZ(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        #endregion

        public static void AddHeaders(this IHeaderDictionary headers)
        {
            if (headers["P3P"].IsNullOrEmpty())
            {
                headers.Add("P3P", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            }
        }

        private static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}