using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation.Errors;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.Extensions;
using Discounting.Entities.TariffDiscounting;
using Discounting.Logics.TariffDiscounting;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace Discounting.Logics.Validators
{
    public interface IDiscountValidator
    {
        Task ValidateAsync(Discount discount, Guid currentUserId);
        Task ValidateSupplyDiscountAsync(Guid discountId, Guid currentUserCompanyId, SupplyDiscount[] supplyDiscounts);
    }

    public class DiscountValidator : IDiscountValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public DiscountValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(Discount discount, Guid currentUserId)
        {
            if (discount.Registry == null)
            {
                throw new NotFoundException(typeof(Registry));
            }

            try
            {
                var currentUser = await unitOfWork.GetOrFailAsync(currentUserId,
                    unitOfWork.Set<User>()
                        .Include(c => c.Company)
                        .ThenInclude(cc => cc.CompanySettings));

                if (!discount.HasChanged &&
                    !await unitOfWork.Set<Tariff>()
                        .AnyAsync(t => t.User.CompanyId == discount.Registry.Contract.BuyerId))
                {
                    throw new NotFoundException(typeof(Tariff));
                }

                if (discount.Id == default)
                {
                    if (!discount.Registry.Contract.IsDynamicDiscounting ||
                        discount.Registry.FinanceType != FinanceType.DynamicDiscounting)
                    {
                        throw new ValidationException(nameof(Registry.ContractId),
                            "You are not allowed to create discounting because the current registries finance type is not dynamic discounting",
                            new ForbiddenErrorDetails("contract-finance-type-is-not-set-to-discounting"));
                    }

                    if (discount.Registry.Contract.SellerId != currentUser.CompanyId)
                    {
                        throw new ForbiddenException("not-seller",
                            "Cannot add discount because current user is not a seller");
                    }

                    //Payment date validation
                    await ValidatePlannedPaymentDateAsync(discount);
                }
                else
                {
                    if (discount.Registry.Contract.SellerId == currentUser.CompanyId)
                    {
                        if (currentUser.Company.CompanySettings.ForbidSellerEditTariff)
                        {
                            throw new ValidationException(nameof(Registry.ContractId),
                                "Seller cannot edit discount",
                                new ForbiddenErrorDetails("seller-cannot-edit-discount"));
                        }

                        if (discount.Registry.IsConfirmed)
                        {
                            throw new ValidationException(nameof(Registry.ContractId),
                                "You cannot edit discount after it was confirmed by buyer",
                                new ForbiddenErrorDetails("seller-cannot-edit-discount.Registry-is-confirmed"));
                        }
                    }

                    //Validate if payment date was changed
                    if (await unitOfWork.Set<Discount>().AnyAsync(d =>
                        d.Id == discount.Id &&
                        d.PlannedPaymentDate.Date != discount.PlannedPaymentDate.Date))
                    {
                        //Payment date validation
                        await ValidatePlannedPaymentDateAsync(discount);
                    }
                }
            }
            catch
            {
                if (discount.Id == default)
                {
                    var supplies = await unitOfWork.Set<Supply>()
                        .Where(s => s.RegistryId == discount.RegistryId)
                        .ToListAsync();
                    supplies.ForEach(s => s.Status = SupplyStatus.InProcess);
                    unitOfWork.Set<Supply>().UpdateRange(supplies);
                    await unitOfWork.RemoveAndSaveAsync<Registry, Guid>(discount.RegistryId);
                }

                throw;
            }
        }

        private async Task ValidatePlannedPaymentDateAsync(Discount discount)
        {
            var buyerDiscountSettings = await unitOfWork.Set<DiscountSettings>()
                .SingleOrDefaultAsync(d => d.CompanyId == discount.Registry.Contract.BuyerId);

            if (buyerDiscountSettings is null)
            {
                throw new NotFoundException(typeof(DiscountSettings));
            }

            var freeDays = unitOfWork.Set<FreeDay>()
                .Where(f => f.IsActive)
                .Select(f => f.Date)
                .ToHashSet();
            ValidatePlannedPaymentDate(discount.PlannedPaymentDate, buyerDiscountSettings, freeDays);
        }

        public void ValidatePlannedPaymentDate(
            DateTime plannedPaymentDate,
            DiscountSettings buyerDiscountSettings,
            HashSet<DateTime> freeDays
        )
        {
            if (buyerDiscountSettings is null)
            {
                throw new NotFoundException(typeof(DiscountSettings));
            }

            if (!buyerDiscountSettings.PaymentWeekDays.HasFlag(plannedPaymentDate.DayOfWeek.ToFlag()))
            {
                throw new ValidationException(nameof(Discount.PlannedPaymentDate),
                    "Planned payment date should match the day of payments",
                    new GeneralErrorDetails("planned-date-wrong-day-of-the-week"));
            }

            if (buyerDiscountSettings.DaysType == DaysType.Calendar)
            {
                if (DateTime.UtcNow.AddDays(buyerDiscountSettings.MinimumDaysToShift).Date > plannedPaymentDate.Date)
                {
                    throw new ValidationException(nameof(Discount.PlannedPaymentDate),
                        "Planned payment date should be later than today + minimum days to shift",
                        new GeneralErrorDetails("planned-date-less-than-minimum-days-shift"));
                }
            }
            else
            {
                var daysCount = 0;
                var daysToShift = buyerDiscountSettings.MinimumDaysToShift;
                var loopCount = 0;
                while (daysToShift > 0)
                {
                    if (IsFreeDay(freeDays, DateTime.UtcNow.AddDays(daysCount)))
                    {
                        daysToShift--;
                    }
                    
                    daysCount++;
                    loopCount++;
                    if(loopCount > 366)
                        break;
                }

                if (DateTime.UtcNow.AddDays(daysCount).Date > plannedPaymentDate.Date)
                {
                    throw new ValidationException(nameof(Discount.PlannedPaymentDate),
                        "Planned payment date should be later than today + minimum days to shift",
                        new GeneralErrorDetails("planned-date-less-than-minimum-days-shift"));
                }
            }
        }

        private static bool IsFreeDay(HashSet<DateTime> freeDays, DateTime date) =>
            freeDays.All(d => d.Date.Date != date.Date);

        public async Task ValidateSupplyDiscountAsync(Guid discountId, Guid currentUserCompanyId,
            SupplyDiscount[] supplyDiscounts)
        {
            var discount = await unitOfWork.GetOrFailAsync(discountId,
                unitOfWork.Set<Discount>()
                    .Include(d => d.Registry)
                    .ThenInclude(r => r.Contract));
            var maxDelayDate = DateTime.MinValue;
            try
            {
                if (discount.Registry.Contract.SellerId != currentUserCompanyId &&
                    discount.Registry.Contract.BuyerId != currentUserCompanyId)
                {
                    throw new ForbiddenException();
                }


                foreach (var supplyDiscount in supplyDiscounts)
                {
                    var supply = await unitOfWork.Set<Supply>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == supplyDiscount.SupplyId);
                    if (supply is null)
                    {
                        throw new NotFoundException(supplyDiscount.SupplyId.ToString());
                    }

                    if (supply.DelayEndDate.Date > maxDelayDate.Date)
                    {
                        maxDelayDate = supply.DelayEndDate;
                    }
                }

                if (maxDelayDate.Date < discount.PlannedPaymentDate.Date)
                {
                    throw new ValidationException(nameof(Discount.PlannedPaymentDate),
                        "Planned payment date should be smaller than max delay date",
                        new GeneralErrorDetails("planned-date-higher-than-max-delay-date"));
                }
            }
            catch
            {
                // This is done because currently frontend creates Discount entity first than SupplyDiscount
                if (discount.Registry != null)
                {
                    var supplies = await unitOfWork.Set<Supply>()
                        .Where(s => s.RegistryId == discount.RegistryId)
                        .ToListAsync();
                    supplies.ForEach(s => s.Status = SupplyStatus.InProcess);
                    unitOfWork.Set<Supply>().UpdateRange(supplies);
                    await unitOfWork.RemoveAndSaveAsync<Registry, Guid>(discount.RegistryId);
                }

                throw;
            }
        }
    }
}