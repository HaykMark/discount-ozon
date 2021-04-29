using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators.Account
{
    public interface IUserValidator
    {
        Task ValidateUserAsync(User user);
        Task ValidateUserAsync(User user, User currentUser);
        void ValidateDeactivation(User user, User currentUser);
        void ValidateActivation(User user, User currentUser);
    }

    public class UserValidator : IUserValidator
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICommonValidations commonValidations;

        public UserValidator(IUnitOfWork unitOfWork, ICommonValidations commonValidations)
        {
            this.unitOfWork = unitOfWork;
            this.commonValidations = commonValidations;
        }

        public async Task ValidateUserAsync(User user)
        {
            commonValidations.ValidatePhoneNumber(user.Phone);
            var entity = await unitOfWork.Set<User>().FirstOrDefaultAsync(u => u.Id == user.Id);
            if (entity == null)
            {
                if (user.IsSuperAdmin || user.IsConfirmedByAdmin || user.IsActive || user.IsEmailConfirmed)
                {
                    throw new ForbiddenException();
                }

                await ValidateUserRegistrationAsync(user);
            }
        }

        public async Task ValidateUserAsync(User user, User currentUser)
        {
            commonValidations.ValidatePhoneNumber(user.Phone);
            await ValidateEmailUniqueness(user);

            if (!currentUser.IsSuperAdmin)
            {
                if (!currentUser.IsAdmin || !currentUser.IsConfirmedByAdmin)
                {
                    throw new ForbiddenException("only-confirmed-admin-can-add-user",
                        "Only confirmed Admin users can add a new user");
                }

                var userExists = await unitOfWork.Set<User>().AnyAsync(u => u.Id == user.Id);
                if (!userExists &&
                    (user.IsSuperAdmin || user.IsConfirmedByAdmin || user.IsAdmin || user.IsEmailConfirmed))
                {
                    throw new ForbiddenException("only-superadmin-can-change-admin-fields",
                        "Only SuperAdmin can change admin fields");
                }

                if (await unitOfWork.Set<User>()
                    .AnyAsync(u => u.Id == user.Id &&
                                   (u.IsSuperAdmin != user.IsSuperAdmin ||
                                    u.IsAdmin != user.IsAdmin ||
                                    u.IsConfirmedByAdmin != user.IsConfirmedByAdmin))
                )
                {
                    throw new ForbiddenException("only-superadmin-can-change-admin-fields",
                        "Only SuperAdmin can change admin fields");
                }
            }
            else if (!await unitOfWork.Set<Company>().AnyAsync(c => c.Id == user.CompanyId))
            {
                throw new NotFoundException(typeof(Company));
            }
        }

        private async Task ValidateUserRegistrationAsync(User user)
        {
            if (await unitOfWork.Set<User>().AnyAsync(c => c.Company.TIN.Equals(user.Company.TIN)))
            {
                throw new ForbiddenException("company-already-has-user",
                    "Current company already has a admin user.");
            }
        }

        public void ValidateDeactivation(User user, User currentUser)
        {
            if (!user.IsActive)
            {
                throw new ForbiddenException("user-is-already-deactivated",
                    "This user is already deactivated");
            }

            if (!currentUser.IsSuperAdmin && currentUser.CompanyId != user.CompanyId)
            {
                throw new ForbiddenException("not-your-company-user-to-deactivate",
                    "User can deactivate only their company users");
            }

            if (currentUser.Id == user.Id)
            {
                throw new ForbiddenException("not-allowed-to-deactivate-yourself",
                    "User can deactivate only other users");
            }
        }

        public void ValidateActivation(User user, User currentUser)
        {
            if (user.IsActive)
            {
                throw new ForbiddenException("user-is-already-active",
                    "This user is already deactivated");
            }

            if (!currentUser.IsSuperAdmin && currentUser.CompanyId != user.CompanyId)
            {
                throw new ForbiddenException("not-your-company-user-to-deactivate",
                    "User can deactivate only their company users");
            }

            if (currentUser.Id == user.Id)
            {
                throw new ForbiddenException("not-allowed-to-activate-yourself",
                    "User can activate only other users");
            }
        }


        private async Task ValidateEmailUniqueness(User user)
        {
            var userToCheckEmailUniqueness = user;
            var any = await unitOfWork.Set<User>().AnyAsync(u => u.Id == user.Id);
            if (any)
            {
                userToCheckEmailUniqueness = await unitOfWork.GetOrFailAsync<User, Guid>(user.Id);
            }

            if (userToCheckEmailUniqueness.Email != user.Email || !any)
            {
                if (await unitOfWork
                    .Set<User>()
                    .AnyAsync(x => x.Email.ToLower() == user.Email.ToLower())
                )
                {
                    throw new ForbiddenException("email-is-taken",
                        "Email address already exists. Please provide a new email address.");
                }
            }
        }
    }
}