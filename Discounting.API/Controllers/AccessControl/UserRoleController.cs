using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.AccessControl;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Logics.AccessControl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Discounting.Common.AccessControl.Zones;

namespace Discounting.API.Controllers.AccessControl
{
    /// <summary>
    ///     Represents http api endpoints for the user management.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(Routes.User.Roles)]
    [Zone(UserRoles)]
    public class UserRoleController : BaseController
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public UserRoleController(
            IMapper mapper,
            IFirewall firewall,
            IUnitOfWork unitOfWork
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ResourceCollection<UserRoleDTO>> Get(
            Guid? userId = null)
        {
            return mapper.Map<ResourceCollection<UserRoleDTO>>(
                await unitOfWork
                    .Set<UserRole>()
                    .Where(ur => userId == null
                                 || ur.UserId == userId)
                    .ToListAsync()
            );
        }
    }
}