using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Logics.Validators.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discounting.Logics.Account
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRole>> SetRolesAsync(Guid userId, Guid[] roleIds);
        Task<IEnumerable<UserRole>> SetUsersAsync(Guid roleId, Guid[] userIds);
    }

    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly DbSet<UserRole> dbSet;
        private readonly IUserRoleValidator userRoleValidator;
        private readonly ILogger<UserRoleService> logger;

        public UserRoleService(
            IUnitOfWork unitOfWork,
            IUserRoleValidator userRoleValidator,
            ILogger<UserRoleService> logger
        )
        {
            dbSet = unitOfWork.Set<UserRole>();
            this.unitOfWork = unitOfWork;
            this.userRoleValidator = userRoleValidator;
            this.logger = logger;
        }

        /// <summary>
        /// Sets roles for a user.
        /// </summary>
        /// <remarks>
        /// * If a role is included as argument and the relation already exists,
        ///   it is not modified and left untouched.
        /// * If a role is included as argument but does not exist, it is newly added.
        /// * If a role is not included as argument but exists in db then it is removed.
        /// </remarks>
        public async Task<IEnumerable<UserRole>> SetRolesAsync(Guid userId, Guid[] roleIds)
        {
            var user = await unitOfWork.GetOrFailAsync(userId, unitOfWork.Set<User>()
                .Include(u => u.Company));

            await userRoleValidator.ValidateAsync(user, roleIds);
            var curUserRoles = await dbSet
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var addedRoles = roleIds
                .Except(curUserRoles.Select(ur => ur.RoleId))
                .ToList();


            unitOfWork.RemoveRange(curUserRoles.Where(ur => !roleIds.Contains(ur.RoleId)));

            unitOfWork.AddRange(addedRoles.Select(roleId =>
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                })
            );

            await unitOfWork.SaveChangesAsync();

            return dbSet.Where(ur => ur.UserId == userId);
        }

        public async Task<IEnumerable<UserRole>> SetUsersAsync(Guid roleId, Guid[] userIds)
        {
            var role = await unitOfWork.GetOrFailAsync<Role, Guid>(roleId);
            await userRoleValidator.ValidateAsync(role, userIds);
            var roles = await dbSet
                .Where(e => e.RoleId == roleId)
                .ToListAsync();
            var rolesToRemove = roles.Where(ur => !userIds.Contains(ur.UserId)).ToList();
            var rolesToAdd = userIds.Except(roles.Select(ur => ur.UserId));
            unitOfWork.RemoveRange(rolesToRemove);

            unitOfWork.AddRange(rolesToAdd.Select(userId =>
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                })
            );

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation($"New Role: {role.Name} where set for the Users: {string.Join(',', userIds)}");
            logger.LogInformation($"Role: {role.Name} was removed from the Users: {string.Join(',', userIds)}");
            return dbSet.Where(ur => ur.RoleId == roleId);
        }
    }
}