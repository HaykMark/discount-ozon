using System;
using System.Linq;
using System.Security.Claims;
using Discounting.Logics.AccessControl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers
{
    [Authorize]
    [EnableCors("CorsPolicy")]
    public class BaseController : ControllerBase
    {
        protected readonly IFirewall firewall;

        public BaseController(
            IFirewall firewall = null
        )
        {
            this.firewall = firewall;
        }

        protected string GetWebRootPath()
        {
            var hostingEnvironment =
                HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
            return hostingEnvironment.WebRootPath;
        }


        protected Guid GetUserId()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userIdValue = claimsIdentity?.FindFirst(ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrWhiteSpace(userIdValue) && Guid.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return default;
        }

        protected string GetIpAddress()
        {
            return Request.Headers.TryGetValue("ipaddress", out var ipAddress)
                ? ipAddress.FirstOrDefault()
                : null;
        }
    }
}