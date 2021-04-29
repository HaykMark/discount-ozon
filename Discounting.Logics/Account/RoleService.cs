using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation.Errors;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Account
{
    public interface IRoleService
    {
        IQueryable<Role> GetBaseQuery();
        Task<List<Role>> GetRolesByUserAsync(Guid userId);
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAndSaveAsync(Role role);
        Task<List<Role>> UpdateRangeAndSaveAsync(List<Role> roles);
    }

    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IQueryable<Role> GetBaseQuery() =>
            unitOfWork
                .Set<Role>()
                .Include(r => r.Permissions);

        public Task<List<Role>> GetRolesByUserAsync(Guid userId)
        {
            return GetBaseQuery()
                .Where(u => u.UserRoles.Any(ur => ur.UserId == userId))
                .ToListAsync();
        }


        public async Task<Role> CreateAsync(Role role)
        {
            await ValidateAsync(role);
            await unitOfWork.AddAndSaveAsync(role);

            return GetBaseQuery().First(r => r.Id == role.Id);
        }

        private async Task UpdateAsync(Role role)
        {
            await ValidateAsync(role);

            var prevPermissions = unitOfWork.Set<Permission>()
                .Where(p => p.RoleId == role.Id);

            unitOfWork.Set<Permission>().RemoveRange(prevPermissions);
        }

        public async Task<Role> UpdateAndSaveAsync(Role role)
        {
            await UpdateAsync(role);

            await unitOfWork.UpdateAndSaveAsync<Role, Guid>(role);

            return GetBaseQuery().First(r => r.Id == role.Id);
        }

        private async Task ValidateAsync(Role entity)
        {
            if (
                await unitOfWork
                    .Set<Role>()
                    .AnyAsync(r =>r.Name == entity.Name
                                  && r.Id != entity.Id))
            {
                throw new ValidationException(nameof(Role.Name),
                    "There is already a role with the same name in the system",
                    new DuplicateErrorDetails());
            }
        }

        public async Task<List<Role>> UpdateRangeAndSaveAsync(List<Role> roles)
        {
            foreach (var r in roles)
            {
                await UpdateAsync(r);
            }
            unitOfWork.UpdateRange(roles);
            await unitOfWork.SaveChangesAsync();

            var roleIds = roles.Select(r => r.Id).ToList();

            return await GetBaseQuery()
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync();
        }
    }
}