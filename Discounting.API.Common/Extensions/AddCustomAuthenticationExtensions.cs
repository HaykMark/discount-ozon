using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Discounting.API.Common.Services;
using Discounting.Common.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Discounting.API.Common.Extensions
{
    /// <summary>
    /// Extensions for custom authentication.
    /// </summary>
    public static class AddCustomAuthenticationExtensions
    {
        /// <summary>
        /// Custom JWT bearer authentication and authorization.
        /// </summary>
        public static IServiceCollection AddCustomAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            var (roleClaimType, nameClaimType) = (
                configuration["BearerTokens:RoleClaimType"],
                configuration["BearerTokens:NameClaimType"]
            );

            // Needed for jwt auth.
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(ops =>
                {
                    ops.RequireHttpsMetadata = false;
                    ops.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["BearerTokens:Issuer"], // site that makes the token
                        ValidateIssuer = false, // TODO: change this to avoid forwarding attacks
                        ValidAudience = configuration["BearerTokens:Audience"], // site that consumes the token
                        ValidateAudience = false, // TODO: change this to avoid forwarding attacks
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["BearerTokens:Key"])),
                        ValidateIssuerSigningKey = true, // verify signature to avoid tampering
                        ValidateLifetime = true, // validate the expiration
                        ClockSkew = TimeSpan.Zero, // tolerance for the expiration date
                        RoleClaimType = roleClaimType,
                        NameClaimType = nameClaimType,
                    };
                    ops.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILoggerFactory>()
                                .CreateLogger(nameof(JwtBearerEvents));
                            logger.LogError("Authentication failed.", context.Exception);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            var tvs = context.HttpContext.RequestServices
                                .GetRequiredService<ITokenValidatorService>();
                            await tvs.ValidateAsync(context);
                        },
                        OnMessageReceived = context => Task.CompletedTask,
                        OnChallenge = context =>
                        {
                            if (context.Error != null)
                            {
                                var logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger($"{nameof(JwtBearerEvents)}.{nameof(JwtBearerEvents.OnChallenge)}");
                                logger.LogError($"Error {context.Error}", context.ErrorDescription);
                            }

                            if (context.AuthenticateFailure is SecurityTokenExpiredException)
                            {
                                throw new TokenExpiredException();
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}