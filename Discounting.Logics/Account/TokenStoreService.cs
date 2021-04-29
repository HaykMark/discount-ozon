using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Discounting.Common.Helpers;
using Discounting.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Discounting.Entities.Account;
using Microsoft.IdentityModel.Tokens;

namespace Discounting.Logics.Account
{
    public interface ITokenStoreService
    {
        Task AddUserTokenAsync(UserToken userToken);
        Task AddUserTokenAsync(User user, string refreshToken, string accessToken, string oldRefreshToken);
        Task DeleteExpiredTokensAsync();
        Task<UserToken> FindTokenAsync(string refreshToken);
        Task DeleteTokenAsync(string refreshToken);
        Task DeleteTokensWithSameRefreshTokenSourceAsync(string refreshTokenIdHashSource);
        Task InvalidateUserTokensAsync(Guid userId);
        Task<(string accessToken, string refreshToken)> CreateJwtTokens(User user, string oldRefreshToken);
        Task RevokeUserBearerTokensAsync(Guid userId, string refreshToken);
    }

    public class TokenStoreService : ITokenStoreService
    {
        private readonly IOptionsSnapshot<BearerTokensOptions> configuration;
        private readonly IRoleService rolesService;
        private readonly IUnitOfWork unitOfWork;
        private readonly DbSet<UserToken> baseDbSet;

        public TokenStoreService(
            IRoleService rolesService,
            IOptionsSnapshot<BearerTokensOptions> configuration,
            IUnitOfWork unitOfWork
            )
        {
            this.rolesService = rolesService;
            this.configuration = configuration;
            this.unitOfWork = unitOfWork;
            baseDbSet = unitOfWork.Set<UserToken>();
        }

        public async Task AddUserTokenAsync(UserToken userToken)
        {
            if (!configuration.Value.AllowMultipleLoginsFromTheSameUser)
            {
                await InvalidateUserTokensAsync(userToken.UserId);
            }

            await DeleteTokensWithSameRefreshTokenSourceAsync(userToken.RefreshTokenIdHashSource);
            await baseDbSet.AddAsync(userToken);
        }

        public async Task AddUserTokenAsync(User user, string refreshToken, string accessToken,
            string oldRefreshToken)
        {
            var now = DateTimeOffset.UtcNow;
            var userToken = new UserToken
            {
                UserId = user.Id,
                // Refresh token handles should be treated as secrets and should be stored hashed
                RefreshTokenIdHash = SecurityToolkit.GetSha512Hash(refreshToken),
                RefreshTokenIdHashSource = string.IsNullOrWhiteSpace(oldRefreshToken)
                    ? null
                    : SecurityToolkit.GetSha512Hash(oldRefreshToken),
                AccessTokenHash = SecurityToolkit.GetSha512Hash(accessToken),
                RefreshTokenExpiresDateTime =
                    now.AddSeconds(configuration.Value.RefreshTokenExpirationSeconds),
                AccessTokenExpiresDateTime =
                    now.AddSeconds(configuration.Value.AccessTokenExpirationSeconds)
            };
            await AddUserTokenAsync(userToken);
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var expiredTokens = (await baseDbSet.ToListAsync()).Where(x => x.RefreshTokenExpiresDateTime < now);
            baseDbSet.RemoveRange(expiredTokens);
        }

        public async Task DeleteTokenAsync(string refreshToken)
        {
            var token = await FindTokenAsync(refreshToken);
            if (token != null)
            {
                baseDbSet.Remove(token);
            }
        }

        public Task DeleteTokensWithSameRefreshTokenSourceAsync(string refreshTokenIdHashSource)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenIdHashSource))
            {
                return Task.CompletedTask;
            }
            var tokensWithSource = baseDbSet.Where(t => t.RefreshTokenIdHashSource == refreshTokenIdHashSource);
            baseDbSet.RemoveRange(tokensWithSource);
            return Task.CompletedTask;
        }

        public async Task RevokeUserBearerTokensAsync(Guid userId, string refreshToken)
        {
            if (configuration.Value.AllowSignOutAllUserActiveClients)
            {
                await InvalidateUserTokensAsync(userId);
            }

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var refreshTokenIdHashSource = SecurityToolkit.GetSha512Hash(refreshToken);
                await DeleteTokensWithSameRefreshTokenSourceAsync(refreshTokenIdHashSource);
            }

            await DeleteExpiredTokensAsync();
        }

        public Task<UserToken> FindTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var refreshTokenIdHash = SecurityToolkit.GetSha512Hash(refreshToken);
            return baseDbSet
                .Include(e => e.User)
                .ThenInclude(c => c.Company)
                .FirstOrDefaultAsync(x => x.RefreshTokenIdHash == refreshTokenIdHash);
        }

        public Task InvalidateUserTokensAsync(Guid userId)
        {
            var userTokens = baseDbSet.Where(x => x.UserId == userId);
            baseDbSet.RemoveRange(userTokens);
            return Task.CompletedTask;
        }

        public async Task<(string accessToken, string refreshToken)> CreateJwtTokens(
            User user, string oldRefreshToken)
        {
            var accessToken = await CreateAccessTokenAsync(user);
            var refreshToken = Guid.NewGuid().ToString("N");
            await AddUserTokenAsync(user, refreshToken, accessToken, oldRefreshToken);
            await unitOfWork.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        private async Task<string> CreateAccessTokenAsync(User user)
        {
            var now = DateTimeOffset.UtcNow.AddSeconds(-1);
            var claims = new List<Claim>
            {
                // Unique Id for all Jwt tokes
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Issuer
                new Claim(JwtRegisteredClaimNames.Iss, configuration.Value.Issuer),
                // Issued at
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                // The subject value MUST be unique in the context of the issuer.
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.UserData, user.Id.ToString()),
                // to invalidate the cookie
                new Claim(ClaimTypes.SerialNumber, user.SerialNumber)
            };

            var roleClaimType = configuration.Value.RoleClaimType;
            var roles = await rolesService.GetRolesByUserAsync(user.Id);
            roles.ForEach(role => claims.Add(new Claim(roleClaimType, role.Id.ToString())));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Value.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration.Value.Issuer,
                audience: configuration.Value.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: now.AddSeconds(configuration.Value.AccessTokenExpirationSeconds).UtcDateTime,
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}