using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Email;
using Discounting.API.Common.Options;
using Discounting.API.Common.Services;
using Discounting.API.Common.ViewModels.AccessControl;
using Discounting.API.Common.ViewModels.Account;
using Discounting.API.Common.ViewModels.Common;
using Discounting.API.Common.ViewModels.Company;
using Discounting.API.Common.ViewModels.EmailNotification;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.Auditing;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Account;
using Discounting.Logics.CompanyServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Discounting.Common.AccessControl.Operations;
using static Discounting.Common.AccessControl.Zones;

namespace Discounting.API.Controllers.AccessControl
{
    /// <summary>
    ///     Represents http api endpoints for the user management.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(Routes.User.List)]
    [Zone(Users)]
    public class UsersController : BaseController
    {
        private readonly IAccountService accountService;
        private readonly IMailer mailer;
        private readonly IMapper mapper;
        private readonly IPasswordService passwordService;
        private readonly IRoleService roleService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserRoleService userRoleService;
        private readonly IUserService userService;
        private readonly IAuditService auditService;
        private readonly int maxPasswordRetryLimit;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public UsersController(
            IMapper mapper,
            IFirewall firewall,
            IUserService userService,
            IRoleService roleService,
            IAccountService accountService,
            IUnitOfWork unitOfWork,
            IUserRoleService userRoleService,
            IPasswordService passwordService,
            IOptions<ApiSettingsOptions> apiSettings,
            IMailer mailer,
            IAuditService auditService
        )
            : base(firewall)
        {
            this.mapper = mapper;
            this.userService = userService;
            this.roleService = roleService;
            this.accountService = accountService;
            this.unitOfWork = unitOfWork;
            this.userRoleService = userRoleService;
            this.passwordService = passwordService;
            this.mailer = mailer;
            this.auditService = auditService;
            maxPasswordRetryLimit = apiSettings.Value.PasswordRetryLimit;
        }

        [HttpGet]
        public async Task<ResourceCollection<UserDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            var (users, count) = await userService.GetAll(offset, limit);
            return new ResourceCollection<UserDTO>(mapper.Map<ResourceCollection<UserDTO>>(users), count);
        }

        [HttpGet("{id}")]
        public async Task<UserDTO> Get(Guid id)
        {
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(id);
            return mapper.Map<UserDTO>(user);
        }

        [HttpPost]
        public async Task<UserDTO> Post([FromBody] UserDTO model)
        {
            var user = await userService.CreateUserAsync(mapper.Map<User>(model), maxPasswordRetryLimit);
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.UserAdded,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = user.Id.ToString()
            });
            return mapper.Map<UserDTO>(user);
        }

        [HttpPut("{id}")]
        public async Task<UserDTO> Update(Guid id, [FromBody] UserDTO model)
        {
            var entity = await unitOfWork.GetOrFailAsync<User, Guid>(id);
            mapper.Map(model, entity);
            return mapper.Map<UserDTO>(await userService.UpdateUserAsync(entity));
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await userService.RemoveAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/roles")]
        [DisableControllerZone]
        public async Task<ResourceCollection<RoleDTO>> GetRoles(Guid id)
        {
            await firewall.RequiresAsync(Read, UserRoles);
            await firewall.RequiresAsync(context =>
                context.User.Id == id ||
                context.HasPermission(Read, Roles)
            );

            return mapper.Map<ResourceCollection<RoleDTO>>(
                await roleService.GetRolesByUserAsync(id)
            );
        }

        /// <summary>
        ///     Assign roles to user by providing the respective role ids
        /// </summary>
        /// <remarks>
        ///     * If a role is included as argument and the relation already exists,
        ///     it is not modified and left untouched.
        ///     * If a role is included as argument but does not exist, it is newly added.
        ///     * If a role is not included as argument but exists in db then it is removed.
        ///     <remarks>
        [HttpPost("{id}/user-roles")]
        [DisableControllerZone]
        public async Task<ResourceCollection<UserRoleDTO>> SetRoles(Guid id, [FromBody] Guid[] roleIds)
        {
            await firewall.RequiresAsync(Operations.Update, UserRoles);

            return mapper.Map<ResourceCollection<UserRoleDTO>>(
                await userRoleService.SetRolesAsync(id, roleIds)
            );
        }

        /// <summary>
        ///     Get user's Company
        /// </summary>
        [HttpGet("{id}/company")]
        public async Task<CompanyDTO> GetCompany(Guid id)
        {
            await firewall.RequiresAsync(Operations.Update, Companies);
            var user = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id);
            return mapper.Map<CompanyDTO>(user.Company);
        }

        /// <summary>
        ///     Send an email in order to Reset the password to the user identify by the UserId in the UserActivationDTO.
        ///     This can be used only by an Admin user
        /// </summary>
        [HttpPost("send-activation")]
        public async Task<NoContentResult> SendActivation([FromBody] UserActivationDTO activationDto)
        {
            await accountService.SendActivationAsync(activationDto);
            return NoContent();
        }

        [HttpPost("{id}/password")]
        public async Task<NoContentResult> ChangePassword(Guid id, [FromBody] PasswordDTO passwordDto)
        {
            var user = await passwordService.ChangePasswordAsync(id, passwordDto.Password);
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

        [HttpPost("{id}/deactivate")]
        public async Task<NoContentResult> DeactivateCompany(Guid id, [FromBody] DeactivationDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            await userService.DeactivateAsync(id, model.DeactivationReason);
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.UserBlocked,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = id.ToString()
            });
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<NoContentResult> ActivateCompany(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            await userService.ActivateAsync(id, maxPasswordRetryLimit);
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.UserUnblocked,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = id.ToString()
            });
            return NoContent();
        }

        [HttpPost("send-email-notification")]
        public async Task<NoContentResult> SendActivation([FromBody] UserEmailNotificationDTO model)
        {
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(model.Id);
            await mailer.SendUserEmailAsync(user, model.Type);
            return NoContent();
        }
    }
}