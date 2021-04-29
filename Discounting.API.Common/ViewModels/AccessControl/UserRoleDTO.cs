using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.AccessControl
{
    public class UserRoleDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid UserId { get; set; }
        [CustomRequired]
        public Guid RoleId { get; set; }
    }
}