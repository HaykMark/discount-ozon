using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Data.Context;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.TariffDiscounting;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface IDiscountSettingsValidator
    {
        Task ValidateAsync(DiscountSettings settings, Guid companyId);
    }

    public class DiscountSettingsValidator : IDiscountSettingsValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public DiscountSettingsValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(DiscountSettings settings, Guid companyId)
        {
            if (companyId != settings.CompanyId)
            {
                throw new ForbiddenException();
            }
            
            if (settings.PaymentWeekDays == DaysOfWeek.None)
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(DiscountSettings.PaymentWeekDays)));
            }
            
            if (settings.DaysType == DaysType.None)
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(DiscountSettings.DaysType)));
            }

            if (!await unitOfWork.Set<Company>().AnyAsync(c =>
                c.Id == settings.CompanyId &&
                c.IsActive &&
                c.CompanyType == CompanyType.SellerBuyer)
            )
            {
                throw new NotFoundException(typeof(Company));
            }

            if (await unitOfWork.Set<DiscountSettings>()
                .AnyAsync(d => d.CompanyId == settings.CompanyId && d.Id != settings.Id))
            {
                throw new ForbiddenException("discount-settings-already-exists",
                    "Current company already has a discount settings set");
            }
        }
    }
}