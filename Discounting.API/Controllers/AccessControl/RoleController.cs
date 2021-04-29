using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.AccessControl;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Discounting.Common.AccessControl.Zones;

namespace Discounting.API.Controllers.AccessControl
{
    /// <summary>
    ///     Represents http api endpoints for the user management.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(Routes.Roles)]
    [Zone(Roles)]
    public class RoleController : BaseController
    {
        private readonly IMapper mapper;
        private readonly IRoleService roleService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserRoleService userRoleService;
        private readonly ILogger<RoleController> logger;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public RoleController(
            IMapper mapper,
            IFirewall firewall,
            IUnitOfWork unitOfWork,
            IRoleService roleService,
            IUserRoleService userRoleService,
            ILogger<RoleController> logger
        )
            : base(firewall)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.roleService = roleService;
            this.userRoleService = userRoleService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ResourceCollection<RoleDTO>> GetAll(
            int offset = Filters.Offset,
            int limit = Filters.Limit)
        {
            return new ResourceCollection<RoleDTO>(mapper.Map<ResourceCollection<RoleDTO>>(
                    await roleService
                        .GetBaseQuery()
                        .OrderBy(r => r.Name)
                        .Skip(offset)
                        .Take(limit)
                        .ToListAsync()),
                await roleService.GetBaseQuery().CountAsync());
        }

        [HttpGet("{id}")]
        public async Task<RoleDTO> Get(Guid id)
        {
            return mapper.Map<RoleDTO>(
                await unitOfWork.GetOrFailAsync(
                    id,
                    roleService.GetBaseQuery())
            );
        }

        [HttpPost]
        public async Task<RoleDTO> Create([FromBody] RoleDTO dto)
        {
            var roleDto = mapper.Map<RoleDTO>(
                await roleService.CreateAsync(mapper.Map<Role>(dto))
            );
            logger.LogInformation($"New role was created. Role:{roleDto.Name}");
            return roleDto;
        }

        [HttpPut("{id}")]
        public async Task<RoleDTO> Update(Guid id, [FromBody] RoleDTO dto)
        {
            var roleDto = mapper.Map<RoleDTO>(
                await roleService.UpdateAndSaveAsync(mapper.Map<Role>(dto))
            );
            logger.LogInformation($"Role was updated. Role:{roleDto.Name}");
            return roleDto;
        }

        [HttpPatch]
        public async Task<ResourceCollection<RoleDTO>> BatchUpdate([FromBody] List<RoleDTO> dto)
        {
            return mapper.Map<ResourceCollection<RoleDTO>>(
                await roleService.UpdateRangeAndSaveAsync(mapper.Map<List<Role>>(dto))
            );
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await unitOfWork.RemoveAndSaveAsync<Role, Guid>(id);
            logger.LogInformation($"Role was deleted. RoleId: {id}");
            return NoContent();
        }

        [HttpGet("{id}/user-roles")]
        [DisableControllerZone]
        public async Task<ResourceCollection<UserRoleDTO>> GetUserRoles(Guid id)
        {
            await firewall.RequiresAsync(Operations.Read, UserRoles);
            return mapper.Map<ResourceCollection<UserRoleDTO>>(
                await unitOfWork.Set<UserRole>()
                    .Where(ur => ur.RoleId == id)
                    .ToListAsync()
            );
        }

        [HttpGet("{id}/users")]
        [DisableControllerZone]
        public async Task<ResourceCollection<UserDTO>> GetUsers(Guid id)
        {
            await firewall.RequiresAsync(Operations.Read, UserRoles);

            return mapper.Map<ResourceCollection<UserDTO>>(
                await unitOfWork.Set<UserRole>()
                    .Where(ur => ur.RoleId == id)
                    .Select(ur => ur.User)
                    .ToListAsync()
            );
        }

        [HttpPost("{id}/user-roles")]
        [DisableControllerZone]
        public async Task<ResourceCollection<UserRoleDTO>> SetUserRoles(
            Guid id,
            [FromBody] Guid[] userIds)
        {
            await firewall.RequiresAsync(Operations.Update, UserRoles);

            return mapper.Map<ResourceCollection<UserRoleDTO>>(
                await userRoleService.SetUsersAsync(id, userIds)
            );
        }
    }
}