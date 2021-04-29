using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.AccessControl;
using Discounting.Common.Exceptions;
using Discounting.Logics.Account;
using Microsoft.Extensions.Logging;

namespace Discounting.Logics.AccessControl
{
    /// <summary>
    /// The verificator is an anonymous function provided in e.g. the body
    /// of a controller action to check if a user can perform certain operations.
    /// </summary>
    /// <seealso cref="IFirewall.RequiresAsync(Verificator)" />
    public delegate bool Verificator(FirewallContext userMeta);

    public interface IFirewall
    {
        /// <summary>
        /// Use this in your controller action to immediately
        /// cancel the flow of execution.
        /// </summary>
        /// <exception cref="UnauthenticatedException">
        /// Will throw an unauthorized exception if the current user is not
        /// authorized yet.
        /// </exception>
        void RequiresSession();

        /// <summary>
        /// Advanced check to verify that the user is allowed to perform a
        /// certain operation.
        /// </summary>
        /// <remarks>
        /// Use this check if you need to chain permissions, need access to
        /// the current user or want to make your check dependent on additional
        /// data.
        /// </remarks>
        /// <exception cref="UnauthenticatedException">
        /// Will throw an unauthorized exception if the current user is not
        /// authorized yet.
        /// </exception>
        Task RequiresAsync(Verificator verifier, string key = null, string message = null);

        /// <summary>
        /// Like <cref="Requires" /> with the subtle difference, that it does
        /// not throw an exception but returns a boolean instead.
        /// </summary>
        /// <param name="userId">
        /// If a user id is provided, the permission check will be performed
        /// on the provided user instead of the current user.
        /// </param>
        Task<bool> HasPermissionAsync(Operations operations, string zoneId, Guid? userId = null);

        /// <summary>
        /// Like <cref="Requires" /> with the subtle difference, that it does
        /// not throw an exception but returns a boolean instead
        /// </summary>
        Task<bool> HasPermissionAsync(Verificator verifier, string message = null);

        /// <summary>
        /// Returns a delegate that can be used to iterate over the current
        /// users roles and check whether permissions for a certain zone
        /// are present.
        /// </summary>
        /// <param name="userId">
        /// If a user id is provided, the PermissionChecker will operate on the
        /// provided user's permissions instead of the current user's.
        /// </param>
        Task<PermissionChecker> GetPermissionCheckerAsync(Guid? userId = null);

        /// <summary>
        /// Simple Check whether the user can perform the given operations
        /// in this zone.
        /// </summary>
        /// <remarks>
        /// Use this check if the only requirement is to whether a user
        /// posses the right for this zone.
        /// </remarks>
        /// <exception cref="UnauthenticatedException">
        /// Will throw an unauthorized exception if the current user is not
        /// authorized yet.
        /// </exception>
        Task RequiresAsync(Operations operations, string zoneId);
    }

    public class Firewall : IFirewall
    {
        private IUserService UserService { get; }

        //Firewall uses ISession to read permissions from db and provides authorization
        private ISessionService SessionService { get; }

        private IRoleService RoleService { get; }

        private ILogger<Firewall> Logger { get; }

        public Firewall(
            IUserService userService,
            IRoleService roleService,
            ISessionService sessionService,
            ILogger<Firewall> logger
        )
        {
            SessionService = sessionService;
            RoleService = roleService;
            UserService = userService;
            Logger = logger;
        }

        public void RequiresSession()
        {
            if (!SessionService.IsUserAuthenticated())
            {
                throw new UnauthenticatedException();
            }
        }

        public async Task RequiresAsync(
            Verificator verificator,
            string key = "forbidden",
            string message = "You do not have the permission to perform this operation.")
        {
            if (!await HasPermissionAsync(verificator))
            {
                throw new ForbiddenException(key, message);
            }
        }

        public async Task<bool> HasPermissionAsync(
            Operations operations,
            string zoneId,
            Guid? userId = null
        )
        {
            var can = await GetPermissionCheckerAsync(userId);
            return can(operations, zoneId);
        }

        public async Task RequiresAsync(Operations operations, string zoneId)
        {
            if (!await HasPermissionAsync(operations, zoneId))
            {
                throw new ForbiddenException();
            }
        }

        public async Task<PermissionChecker> GetPermissionCheckerAsync(Guid? userId = null)
        {
            if (userId == null)
            {
                if (SessionService.IsUserAuthenticated())
                {
                    userId = SessionService.GetCurrentUserId();
                }
                else
                {
                    // permission check always returns false for non-authenticated users
                    return (ops, zoneId) => false;
                }
            }

            var currentUser = await UserService.GetCurrentUserAsync();
            if (currentUser.IsSuperAdmin)
            {
                return (ops, zoneId) => true;
            }

            var roles = await RoleService.GetRolesByUserAsync((Guid) userId);

            return (ops, zoneId) =>
                roles.Any(r =>
                    r.Permissions.Any(p =>
                        p.ZoneId == zoneId && (p.Operations & ops) == ops
                    )
                );
        }

        public async Task<bool> HasPermissionAsync(Verificator verifier, string message = null)
        {
            // throws if unauthenticated
            RequiresSession();

            return verifier(new FirewallContext
            {
                User = await UserService.GetCurrentUserAsync(),
                HasPermission = await GetPermissionCheckerAsync()
            });
        }
    }
}