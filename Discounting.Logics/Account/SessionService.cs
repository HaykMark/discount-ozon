using System;
using System.Security.Claims;

namespace Discounting.Logics.Account
{
    public interface ISessionService
    {
        ClaimsPrincipal GetClaimsPrincipal();

        Guid GetCurrentUserId();

        bool IsUserAuthenticated();
    }
}
