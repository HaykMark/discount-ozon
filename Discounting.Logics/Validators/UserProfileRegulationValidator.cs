using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.Regulations;
using Discounting.Logics.Account;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface IUserProfileRegulationInfoValidator
    {
        void ValidateFields(UserProfileRegulationInfo entity);
    }

    public class UserProfileRegulationInfoValidator : IUserProfileRegulationInfoValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public UserProfileRegulationInfoValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void ValidateFields(UserProfileRegulationInfo entity)
        {
            if (entity.IsResident)
            {
                ValidateResidentFields(entity);
            }
            else
            {
                ValidateNonResidentFields(entity);
            }
        }

        private void ValidateResidentFields(UserProfileRegulationInfo entity)
        {
            if (string.IsNullOrEmpty(entity.PassportSeries))
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(entity.PassportSeries)));
            }

            if (string.IsNullOrEmpty(entity.PassportNumber))
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(entity.PassportNumber)));
            }

            if (string.IsNullOrEmpty(entity.PassportUnitCode))
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(entity.PassportUnitCode)));
            }

            if (string.IsNullOrEmpty(entity.PassportIssuingAuthorityPSRN))
            {
                throw new ValidationException(
                    new RequiredFieldValidationError(nameof(entity.PassportIssuingAuthorityPSRN)));
            }

            if (!entity.PassportDate.HasValue)
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(entity.PassportDate)));
            }
        }

        private void ValidateNonResidentFields(UserProfileRegulationInfo entity)
        {
            if (string.IsNullOrEmpty(entity.MigrationCardRightToResideDocument))
            {
                throw new ValidationException(
                    new RequiredFieldValidationError(nameof(entity.MigrationCardRightToResideDocument)));
            }

            if (string.IsNullOrEmpty(entity.MigrationCardAddress))
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(entity.MigrationCardAddress)));
            }

            if (string.IsNullOrEmpty(entity.MigrationCardRegistrationAddress))
            {
                throw new ValidationException(
                    new RequiredFieldValidationError(nameof(entity.MigrationCardRegistrationAddress)));
            }

            if (string.IsNullOrEmpty(entity.MigrationCardPhone))
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(entity.MigrationCardPhone)));
            }
        }
    }
}