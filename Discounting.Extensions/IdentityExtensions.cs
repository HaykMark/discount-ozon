using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

namespace Discounting.Extensions
{
    /// <summary>
    /// Extensions for getting user's name and id from user's claims.
    /// </summary>
    public static class IdentityExtensions
    {
        private static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            return identity?.FindFirst(claimType)?.Value;
        }

        private static string GetUserClaimValue(this IIdentity identity, string claimType)
        {
            var claimIdentity = identity as ClaimsIdentity;
            return claimIdentity?.FindFirstValue(claimType);
        }
  
        public static T GetUserId<T>(this IIdentity identity) where T : IConvertible
        {
            var firstValue = identity?.GetUserClaimValue(ClaimTypes.NameIdentifier);
            return firstValue != null
                ? (T)Convert.ChangeType(firstValue, typeof(T), CultureInfo.InvariantCulture)
                : default(T);
        }

        public static string GetUserId(this IIdentity identity)
        {
            return identity?.GetUserClaimValue(ClaimTypes.UserData);
        }
  
        public static string GetUserName(this IIdentity identity)
        {
            return identity?.GetUserClaimValue(ClaimTypes.Name);
        }
    }
}