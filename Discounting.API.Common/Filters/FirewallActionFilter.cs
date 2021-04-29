using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.AccessControl;
using Discounting.Common.Exceptions;
using Discounting.Logics.AccessControl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using static Discounting.Common.AccessControl.Operations;

namespace Discounting.API.Common.Filters
{
    /// <summary>
    /// This filter ensures that a permission check for the requested action
    /// is performed, given that the controller associated with
    /// a certain zone (via the <see cref="Operations" />).
    /// 
    /// In case the controller has been tagged with a zone, the filter
    /// maps the request's http method to one of the
    /// available <see cref="ZoneAttribute" /> and then checks whether the
    /// operation is allowed in the context of the zone.
    /// </summary>
    public class FirewallActionFilter : IAsyncActionFilter
    {
        private static Dictionary<string, Operations> httpMethodMap =>
            new Dictionary<string, Operations> {
                { "POST", Create },
                { "GET", Read },
                { "OPTIONS", Read },
                { "PATCH", Update },
                { "PUT", Update },
                { "DELETE", Delete },
            };

        private IFirewall firewall { get; }

        public FirewallActionFilter(
            IFirewall firewall
        )
        {
            this.firewall = firewall;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var endpointMeta = context.ActionDescriptor.EndpointMetadata;
            if (endpointMeta.Any(meta =>
                meta is AllowAnonymousAttribute
                || meta is DisableControllerZoneAttribute))
            {
                await next();
                return;
            }

            var zoneAttr = (ZoneAttribute) endpointMeta.FirstOrDefault(a => a is ZoneAttribute);
            if (zoneAttr == null)
            {
                await next();
                return;
            }

            var httpMethod = context.HttpContext.Request.Method;
            if (!httpMethodMap.ContainsKey(httpMethod))
            {
                throw new ForbiddenException();
            }

            var userCan = await firewall.GetPermissionCheckerAsync();
            if (!userCan(httpMethodMap[httpMethod], zoneAttr.ZoneId))
            {
                throw new ForbiddenException();
            }

            await next();
        }
    }
}