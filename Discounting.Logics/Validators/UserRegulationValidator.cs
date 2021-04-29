using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.Regulations;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface IUserRegulationValidator
    {
        Task ValidateRequestedRegulationPermissionAsync(Guid id, Guid companyId);
        Task ValidateForProfileTemplateAsync(Guid userProfileId, User currentUser);
        Task ValidateAsync(UserRegulation entity);
        void ValidateProfile(UserProfileRegulationInfo profileRegulationInfo);
    }

    public class UserRegulationValidator : IUserRegulationValidator
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserProfileRegulationInfoValidator profileRegulationInfoValidator;

        public UserRegulationValidator(IUnitOfWork unitOfWork,
            IUserProfileRegulationInfoValidator profileRegulationInfoValidator)
        {
            this.unitOfWork = unitOfWork;
            this.profileRegulationInfoValidator = profileRegulationInfoValidator;
        }

        public async Task ValidateRequestedRegulationPermissionAsync(Guid id, Guid companyId)
        {
            if (!await unitOfWork.Set<UserRegulation>()
                .AnyAsync(u => u.Id == id &&
                               u.User.CompanyId == companyId))
            {
                throw new NotFoundException();
            }
        }

        public async Task ValidateForProfileTemplateAsync(Guid usesRegulationId, User currentUser)
        {
            if (!await unitOfWork.Set<UserRegulation>().AnyAsync(u =>
                u.Id == usesRegulationId &&
                u.Type == UserRegulationType.Profile &&
                (currentUser.IsSuperAdmin || u.User.CompanyId == currentUser.CompanyId))
            )
            {
                throw new NotFoundException(typeof(UserRegulation));
            }
        }

        public async Task ValidateAsync(UserRegulation entity)
        {
            if (!await unitOfWork.Set<User>().AnyAsync(u => u.Id == entity.UserId))
            {
                throw new NotFoundException(typeof(User));
            }
            if (entity.Type == UserRegulationType.Profile)
            {
                if (await unitOfWork.Set<UserRegulation>()
                    .AnyAsync(u => u.Id != entity.Id &&
                                   u.UserId == entity.UserId &&
                                   u.Type == UserRegulationType.Profile))
                {
                    throw new ForbiddenException("user-can-have-only-one-profile-regulation",
                        "User cannot have more than one profile regulation");
                }

                if (await unitOfWork.Set<User>().AnyAsync(u => u.Id == entity.UserId && u.CanSign))
                {
                    if (entity.UserProfileRegulationInfo == null)
                    {
                        throw new ValidationException(
                            new RequiredFieldValidationError(nameof(UserProfileRegulationInfo)));
                    }

                    ValidateProfile(entity.UserProfileRegulationInfo);
                }
            }
        }

        public void ValidateProfile(UserProfileRegulationInfo profileRegulationInfo) =>
            profileRegulationInfoValidator.ValidateFields(profileRegulationInfo);
    }
}