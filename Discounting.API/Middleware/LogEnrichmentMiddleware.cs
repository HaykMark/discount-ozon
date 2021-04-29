using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace Discounting.API.Middleware
{
    public class LogEnrichmentMiddleware
    {
        private readonly RequestDelegate next;

        public LogEnrichmentMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            using (LogContext.Push(
                new PropertyEnricher("UserName", context.User.Identity.Name ?? "anonymous"),
                new PropertyEnricher("IPAddress", context.Connection.RemoteIpAddress)))
            {
                await next(context);
            }
        }
    }
}