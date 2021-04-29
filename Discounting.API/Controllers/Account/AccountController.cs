using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Options;
using Discounting.API.Common.Services;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Data.Context;
using Discounting.Entities.Auditing;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Discounting.API.Controllers.Account
{
    /// <summary>
    ///     Api endpoint for Account data-access management.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(Routes.Account.Base)]
    public class AccountController : BaseController
    {
        private readonly IAccountService accountService;
        private readonly IAuthenticationService authenticationService;
        private readonly IMapper mapper;
        private readonly ITokenStoreService tokenStoreService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IAuditService auditService;

        public AccountController(
            IUserService userService,
            ITokenStoreService tokenStoreService,
            IUnitOfWork unitOfWork,
            IAccountService accountService,
            IMapper mapper,
            IFirewall firewall,
            IAuthenticationService authenticationService,
            IAuditService auditService
        )
            : base(firewall)
        {
            this.userService = userService;
            this.tokenStoreService = tokenStoreService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.accountService = accountService;
            this.authenticationService = authenticationService;
            this.auditService = auditService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<SessionInfoDTO> Login([FromBody] LoginDTO dto)
        {
            var sessionInfoDto = await accountService.LoginAsync(dto.UserIdentifier, dto.Password);
            await auditService.CreateAsync(new Audit
            {
                UserId = sessionInfoDto.User.Id,
                Incident = IncidentType.UserLoggedIn,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = sessionInfoDto.User.Id.ToString()
            });
            return sessionInfoDto;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<SessionInfoDTO> Register([FromBody] RegistrationDTO dto)
        {
            var sessionDto = await accountService.RegisterAsync(dto);
            await auditService.CreateAsync(new Audit
            {
                UserId = sessionDto.User.Id,
                Incident = IncidentType.UserRegistered,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = sessionDto.User.Id.ToString()
            });
            return sessionDto;
        }

        [AllowAnonymous]
        [HttpPost("token/refresh")]
        public async Task<SessionInfoDTO> RefreshToken([FromBody] JToken jsonBody)
        {
            var refreshToken = jsonBody.Value<string>("refreshToken");
            var token = await authenticationService.AuthenticateRefreshTokenAsync(refreshToken);
            return await accountService.GenerateSessionAsync(token.User, refreshToken);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<NoContentResult> Logout([FromBody] JToken jsonBody)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return NoContent();
            }
            var refreshToken = jsonBody.Value<string>("refreshToken");

            // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
            // Delete the user's tokens from the database (revoke its bearer token)
            await tokenStoreService.RevokeUserBearerTokensAsync(userId, refreshToken);
            await unitOfWork.SaveChangesAsync();
            await auditService.CreateAsync(new Audit
            {
                UserId = userId,
                Incident = IncidentType.UserLoggedOut,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = userId.ToString()
            });
            return NoContent();
        }

        [HttpGet]
        public async Task<UserDTO> GetUserInfo()
        {
            var user = await userService.GetCurrentUserAsync();
            return mapper.Map<UserDTO>(user);
        }

        /// <summary>
        ///     Send an email in order to activate the user. Identify by the Email in the UserByEmailActivationDTO.
        ///     This should be used when the user is not logged in
        /// </summary>
        [HttpPost("send-activation-email")]
        [AllowAnonymous]
        public async Task<NoContentResult> SendActivation([FromBody] UserByEmailActivationDTO activationByEmailDto)
        {
            await accountService.SendActivationAsync(activationByEmailDto);

            return NoContent();
        }

        /// <summary>
        ///     Send an email in order to Reset the password to the user identify by the UserId in the CurrentUserActivationDTO.
        ///     This should be called with current user (logged in)
        /// </summary>
        [HttpPost("send-activation-user")]
        public async Task<NoContentResult> SendActivation([FromBody] CurrentUserActivationDTO currentUserActivationDto)
        {
            await accountService.SendActivationAsync(currentUserActivationDto);

            return NoContent();
        }

        /// <summary>
        ///     Validate if the UserId and ConfirmationCode are valid to activate new user
        /// </summary>
        [HttpGet("activate-user")]
        [AllowAnonymous]
        public async Task<SessionInfoDTO> ActivateUser(UserConfirmationCodeDTO userConfirmationCodeDto)
        {
            var sessionDto = await accountService.TryActivateAsync(userConfirmationCodeDto);
            await auditService.CreateAsync(new Audit
            {
                UserId = sessionDto.User.Id,
                Incident = IncidentType.UserEmailConfirmed,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = sessionDto.User.Id.ToString()
            });
            return sessionDto;
        }
    }
}