using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discounting.Common.Helpers;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Logics.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Discounting.API.Common.Services
{
    public interface ITokenValidatorService
    {
        Task ValidateAsync(TokenValidatedContext context);
    }

    public class TokenValidatorService : ITokenValidatorService
    {
        private readonly IUserService userService;
        private readonly IOptionsSnapshot<BearerTokensOptions> tokenOptions;
        private readonly IUnitOfWork unitOfWork;

        public TokenValidatorService(
            IUserService userService,
            IOptionsSnapshot<BearerTokensOptions> tokenOptions,
            IUnitOfWork unitOfWork
        )
        {
            this.userService = userService;
            this.tokenOptions = tokenOptions;
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(TokenValidatedContext context)
        {
            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
            {
                context.Fail("This is not our issued token. It has no claims.");
                return;
            }

            var serialNumberClaim = claimsIdentity.FindFirst(ClaimTypes.SerialNumber);
            if (serialNumberClaim == null)
            {
                context.Fail("This is not our issued token. It has no serial.");
                return;
            }

            var userIdString = claimsIdentity.FindFirst(ClaimTypes.UserData).Value;
            if (!Guid.TryParse(userIdString, out var userId))
            {
                context.Fail("This is not our issued token. It has no user-id.");
                return;
            }

            long.TryParse(claimsIdentity.FindFirst(JwtRegisteredClaimNames.Iat).Value, out var issuedAt);
            var expiredAt = DateTimeOffset.UtcNow
                .AddSeconds(-tokenOptions.Value.AccessTokenExpirationSeconds)
                .ToUnixTimeSeconds();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            if (user.SerialNumber != serialNumberClaim.Value || !user.IsActive || expiredAt >= issuedAt)
            {
                context.Fail("Token is expired. Please login again!");
                return;
            }

            if (!(context.SecurityToken is JwtSecurityToken accessToken)
                || string.IsNullOrWhiteSpace(accessToken.RawData)
                || !await IsValidTokenAsync(accessToken.RawData, userId)
            )
            {
                context.Fail("This token is not in our database.");
                return;
            }

            await userService.UpdateUserLastActivityDateAsync(userId);
        }

        private async Task<bool> IsValidTokenAsync(string accessToken, Guid userId)
        {
            var accessTokenHash = SecurityToolkit.GetSha512Hash(accessToken);
            var userToken = await unitOfWork
                .Set<UserToken>()
                .FirstOrDefaultAsync(
                    x => x.AccessTokenHash == accessTokenHash 
                         && x.UserId == userId);
            return userToken?.AccessTokenExpiresDateTime >= DateTimeOffset.UtcNow;
        }
    }
}