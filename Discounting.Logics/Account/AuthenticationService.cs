using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Entities.Account;
using Microsoft.Extensions.Logging;

namespace Discounting.Logics.Account
{
    public interface IAuthenticationService
    {
        Task<User> AuthenticateUserByEmailAsync(string email, string password, int maxPasswordRetryLimit);

        Task<UserToken> AuthenticateRefreshTokenAsync(string refreshToken);

        bool IsValidEmailConfirmationCode(
            User user,
            string token,
            int activationTokenExpirationDuration
        );
    }

    /// <summary>
    /// AuthenticationService provides some user-account-related operations for which it also needs the firewall
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService userService;
        private readonly ITokenStoreService tokenStoreService;
        private readonly ILogger<AuthenticationService> logger;

        public AuthenticationService(
            ITokenStoreService tokenStoreService,
            IUserService userService,
            ILogger<AuthenticationService> logger
        )
        {
            this.tokenStoreService = tokenStoreService;
            this.userService = userService;
            this.logger = logger;
        }


        public async Task<User> AuthenticateUserByEmailAsync(string userIdentifier, string password, int maxPasswordRetryLimit)
        {
            var user = await userService.FindUserByEmailAsync(userIdentifier, password, maxPasswordRetryLimit);

            await ThrowIfAuthenticationNotPossible(user, userIdentifier);
            return user;
        }

        private Task ThrowIfAuthenticationNotPossible(User user, string userIdentifier)
        {
            if (user == null)
            {
                logger.LogInformation($"Login failed, User identifier: {userIdentifier}");
                throw new NotFoundException(typeof(User));
            }

            if (!user.IsEmailConfirmed)
            {
                throw new ForbiddenException("user-email-is-not-confirmed",
                    "Current users email is not confirmed");
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("user-is-blocked",
                    "Current user is blocked");
            }

            if (!user.IsAdmin)
            {
                if (!user.IsConfirmedByAdmin)
                {
                    throw new ForbiddenException("user-is-not-confirmed-by-admin",
                        "Current users is not confirmed by admin");
                }

                if (!user.Company.IsActive)
                {
                    throw new ForbiddenException("user-company-is-blocked",
                        "Current users company is blocked");
                }
            }
            else if (!user.Company.IsActive && user.IsConfirmedByAdmin)
            {
                throw new ForbiddenException("user-company-is-blocked",
                    "Current users company is blocked");
            }

            return Task.CompletedTask;
        }

        public async Task<UserToken> AuthenticateRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ValidationException(new RequiredFieldValidationError("RefreshToken"));
            }

            var token = await tokenStoreService.FindTokenAsync(refreshToken);
            if (token == null)
            {
                throw new UnauthenticatedException();
            }

            if (token.RefreshTokenExpiresDateTime <= DateTime.UtcNow)
            {
                throw new UnauthenticatedException();
            }

            if (!token.User.IsActive)
            {
                throw new UnauthenticatedException();
            }

            if (!token.User.Company.IsActive)
            {
                throw new UnauthenticatedException();
            }

            return token;
        }

        public bool IsValidEmailConfirmationCode(
            User user,
            string token,
            int activationTokenExpirationDuration
        )
        {
            if (user.ActivationToken == null || user.ActivationToken != token)
            {
                return false;
            }

            if (user.ActivationTokenCreationDateTime != null)
            {
                var diff = DateTime.UtcNow - user.ActivationTokenCreationDateTime.Value;
                if (diff.TotalSeconds > activationTokenExpirationDuration)
                {
                    return false;
                }
            }

            return true;
        }
    }
}