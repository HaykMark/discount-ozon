using System;
using System.Security.Claims;
using Discounting.Logics.Account;
using Microsoft.AspNetCore.Http;

namespace Discounting.API.Common.Services
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal GetClaimsPrincipal()
        {
            var httpContext = httpContextAccessor?.HttpContext;
            return httpContext?.User;
        }

        public Guid GetCurrentUserId()
        {
            var claimsIdentity = httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            var userDataClaim = claimsIdentity?.FindFirst(ClaimTypes.UserData);
            var userId = userDataClaim?.Value;
            return string.IsNullOrWhiteSpace(userId)
                ? default(Guid)
                : Guid.Parse(userId);
        }

        public bool IsUserAuthenticated()
        {
            return httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }
    }
}