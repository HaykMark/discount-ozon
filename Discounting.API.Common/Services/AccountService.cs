using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Email;
using Discounting.API.Common.Options;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Logics;
using Discounting.Logics.Account;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Discounting.API.Common.Services
{
    public interface IAccountService
    {
        Task<SessionInfoDTO> GenerateSessionAsync(User user, string refreshToken = null);

        Task SendActivationAsync(UserByEmailActivationDTO activationByEmailDto);
        Task SendActivationAsync(UserActivationDTO activationDto);
        Task SendActivationAsync(CurrentUserActivationDTO currentUserActivationDto);
        Task<SessionInfoDTO> LoginAsync(string userIdentifier, string password);
        Task<SessionInfoDTO> RegisterAsync(RegistrationDTO registrationDto);
        Task<SessionInfoDTO> TryActivateAsync(UserConfirmationCodeDTO userConfirmationCodeDto);
    }

    public class AccountService : IAccountService
    {
        private readonly ITokenStoreService tokenStoreService;
        private readonly IWebHostEnvironment environment;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly IMailer mailer;
        private readonly IRoleService roleService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuthenticationService authenticationService;
        private readonly int activationTokenExpirationDuration;
        private readonly int maxPasswordRetryLimit;
        private readonly ISupplyService supplyService;

        public AccountService(
            ITokenStoreService tokenStoreService,
            IMapper mapper,
            IWebHostEnvironment environment,
            IUserService userService,
            IMailer mailer,
            IRoleService roleService,
            IUnitOfWork unitOfWork,
            IAuthenticationService authenticationService,
            IOptions<ApiSettingsOptions> apiSettings,
            ISupplyService supplyService
        )
        {
            this.environment = environment;
            this.tokenStoreService = tokenStoreService;
            this.mapper = mapper;
            this.mailer = mailer;
            this.roleService = roleService;
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.authenticationService = authenticationService;
            this.supplyService = supplyService;
            this.userService = userService;
            activationTokenExpirationDuration = apiSettings.Value.ActivationTokenExpirationDuration;
            maxPasswordRetryLimit = apiSettings.Value.PasswordRetryLimit;
        }

        public async Task<SessionInfoDTO> GenerateSessionAsync(User user, string refreshToken = null)
        {
            var roles = await roleService.GetRolesByUserAsync(user.Id);
            var (accessToken, newRefreshToken) = await tokenStoreService.CreateJwtTokens(user, refreshToken);

            //Update current users supplies before logging him in
            await supplyService.UpdateNotAvailableSupplies();

            return new SessionInfoDTO
            {
                User = mapper.Map<UserDTO>(user),
                TokenResource = new TokenDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken
                },
                Roles = mapper.Map<List<RoleDTO>>(roles)
            };
        }

        public async Task SendActivationAsync(UserActivationDTO activationDto)
        {
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(activationDto.UserId);
            await CreateConfirmationEmail(user, activationDto.BaseUrl);
        }

        public async Task SendActivationAsync(UserByEmailActivationDTO activationByEmailDto)
        {
            try
            {
                var user = await userService.FindUserByEmailAsync(activationByEmailDto.Email);
                await CreateConfirmationEmail(user, activationByEmailDto.BaseUrl);
            }
            catch (NotFoundException)
            {
                throw new ForbiddenException();
            }
        }

        private async Task CreateConfirmationEmail(User user, string baseUrl)
        {
            user = await userService.GenerateEmailConfirmationCode(user, baseUrl);

            var callbackUrl = QueryHelpers.AddQueryString(baseUrl,
                new Dictionary<string, string>
                {
                    {
                        "userId",
                        user.Id.ToString()
                    },
                    {
                        "code",
                        user.ActivationToken
                    },
                });

            var emailContent = EmailTemplates.GetConfirmationHtmlString(
                environment,
                user.FirstName,
                user.Surname,
                callbackUrl
            );

            await mailer.SendEmailAsync(new List<string> {user.Email}, "Trade Finance Activation", emailContent);
        }

        public async Task<SessionInfoDTO> LoginAsync(string userIdentifier, string password)
        {
            userIdentifier = userIdentifier.TrimEnd();
            var user = await authenticationService.AuthenticateUserByEmailAsync(userIdentifier, password,
                maxPasswordRetryLimit);

            return await GenerateSessionAsync(user);
        }

        public async Task<SessionInfoDTO> RegisterAsync(RegistrationDTO registrationDto)
        {
            var user = mapper.Map<User>(registrationDto);
            return await GenerateSessionAsync(
                await userService.CreateUserAsync(user, registrationDto.Password, maxPasswordRetryLimit));
        }

        public async Task<SessionInfoDTO> TryActivateAsync(UserConfirmationCodeDTO userConfirmationCodeDto)
        {
            try
            {
                var user = await unitOfWork.GetOrFailAsync<User, Guid>(userConfirmationCodeDto.UserId);
                if (!authenticationService.IsValidEmailConfirmationCode(
                    user,
                    userConfirmationCodeDto.Code,
                    activationTokenExpirationDuration
                ))
                {
                    throw new ForbiddenException();
                }

                return await GenerateSessionAsync(await userService.EmailActivationAsync(user));
            }
            catch (NotFoundException)
            {
                throw new ForbiddenException();
            }
        }

        public async Task SendActivationAsync(CurrentUserActivationDTO currentUserActivationDto)
        {
            try
            {
                var currentUser = await userService.GetCurrentUserAsync();
                await CreateConfirmationEmail(currentUser, currentUserActivationDto.BaseUrl);
            }
            catch (NotFoundException)
            {
                throw new ForbiddenException();
            }
        }
    }
}