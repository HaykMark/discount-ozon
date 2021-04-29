using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators.Account
{
    public interface IUserRoleValidator
    {
        Task ValidateAsync(User userId, Guid[] roleIds);
        Task ValidateAsync(Role roleId, Guid[] userIds);
    }

    public class UserRoleValidator : IUserRoleValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public UserRoleValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(User user, Guid[] roleIds)
        {
            if (!user.Company.IsActive)
            {
                foreach (var roleId in roleIds)
                {
                    if (await unitOfWork.Set<Role>()
                        .AnyAsync(r => r.Id == roleId && r.Type != RoleType.InactiveCompany))
                    {
                        throw new ForbiddenException("can-assign-only-initial-role",
                            "You can only assign Inactive Company role for the newly registered users");
                    }
                }
            }
        }

        public async Task ValidateAsync(Role role, Guid[] userIds)
        {
            foreach (var userId in userIds)
            {
                var user = await unitOfWork.GetOrFailAsync(userId, unitOfWork.Set<User>()
                    .Include(u => u.Company));
                if (!user.Company.IsActive && role.Type != RoleType.InactiveCompany)
                {
                    throw new ForbiddenException("can-assign-only-inactive-company-role",
                        "You can only assign Inactive Company role for the newly registered users");
                }
            }
        }
    }
}