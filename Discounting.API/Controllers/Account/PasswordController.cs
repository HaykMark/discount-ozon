using System;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Options;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Discounting.API.Controllers.Account
{
    [ApiVersion("1.0")]
    [Route(Routes.Account.Password)]
    public class PasswordController : BaseController
    {
        private readonly int activationTokenExpirationDuration;
        private readonly IPasswordService passwordService;
        private readonly ILogger<PasswordController> logger;
        private readonly IAuditService auditService;

        public PasswordController(
            IPasswordService passwordService,
            IFirewall firewall,
            IOptions<ApiSettingsOptions> apiSettings,
            ILogger<PasswordController> logger,
            IAuditService auditService
        )
            : base(firewall)
        {
            this.passwordService = passwordService;
            this.logger = logger;
            this.auditService = auditService;
            activationTokenExpirationDuration = apiSettings.Value.ActivationTokenExpirationDuration;
        }

        [HttpPost("change")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var user = await passwordService.ChangePasswordAsync(model.OldPassword, model.NewPassword);
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.PasswordChanged,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = user.Id.ToString(),
            });

            return NoContent();
        }

        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            await passwordService.ResetPasswordAsync(
                resetPasswordDto.UserId,
                resetPasswordDto.NewPassword,
                resetPasswordDto.ConfirmationCode,
                activationTokenExpirationDuration
            );
            
            await auditService.CreateAsync(new Audit
            {
                UserId = resetPasswordDto.UserId,
                Incident = IncidentType.PasswordChanged,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress()
            });
            return NoContent();
        }

        /// <summary>
        ///     Validate if the UserId and ConfirmationCode are valid to reset the password.
        /// </summary>
        [HttpGet("validate-user-confirmation-code")]
        [AllowAnonymous]
        public async Task<ValidationResultDTO> IsUserConfirmationCodeValid(
            UserConfirmationCodeDTO userConfirmationCodeDto)
        {
            var validationResultDto = new ValidationResultDTO
            {
                IsValid = await passwordService.IsValidUserConfirmationCodeAsync(
                    userConfirmationCodeDto.UserId,
                    userConfirmationCodeDto.Code,
                    activationTokenExpirationDuration)
            };

            return validationResultDto;
        }
    }
}