using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.TariffDiscounting;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.TariffDiscounting
{
    public interface IDiscountSettingsService
    {
        Task<DiscountSettings> GetAsync(Guid id);
        Task<DiscountSettings[]> GetAllAsync(bool onlyCurrent);
        Task<DiscountSettings> CreateAsync(DiscountSettings entity);
        Task<DiscountSettings> UpdateAsync(DiscountSettings entity);
    }

    public class DiscountSettingsService : IDiscountSettingsService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IDiscountSettingsValidator discountSettingsValidator;
        private readonly ISessionService sessionService;

        public DiscountSettingsService(
            IUnitOfWork unitOfWork,
            IDiscountSettingsValidator discountSettingsValidator,
            ISessionService sessionService
        )
        {
            this.unitOfWork = unitOfWork;
            this.discountSettingsValidator = discountSettingsValidator;
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Returned all settings that are allowed for this user
        /// Super admins will get all Settings
        /// Normal users will get only settings for those companies that are connected to this current user
        /// Connection is determined via Contracts
        /// </summary>
        /// <param name="onlyCurrent">If true returns only current user companies DiscountSettings</param>
        /// <returns></returns>
        public async Task<DiscountSettings[]> GetAllAsync(bool onlyCurrent)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);

            if (onlyCurrent)
            {
                var currentUserDiscountSettings = await unitOfWork.Set<DiscountSettings>()
                    .FirstOrDefaultAsync(d => d.CompanyId == currentUser.CompanyId);
                return currentUserDiscountSettings is null
                    ? new DiscountSettings[0]
                    : new[] {currentUserDiscountSettings};
            }

            if (currentUser.IsSuperAdmin)
            {
                return await unitOfWork.Set<DiscountSettings>().ToArrayAsync();
            }

            var allConnectedContracts = await unitOfWork.Set<Contract>()
                .Where(c => c.SellerId == currentUser.CompanyId ||
                            c.BuyerId == currentUser.CompanyId)
                .ToListAsync();
            var companyIds = new HashSet<Guid>();
            allConnectedContracts.ForEach(c =>
            {
                if (!companyIds.Contains(c.SellerId))
                    companyIds.Add(c.SellerId);
                if (!companyIds.Contains(c.BuyerId))
                    companyIds.Add(c.BuyerId);
            });
            return await unitOfWork.Set<DiscountSettings>()
                .Where(d => companyIds.Contains(d.CompanyId))
                .ToArrayAsync();
        }

        public Task<DiscountSettings> GetAsync(Guid id)
            => unitOfWork.GetOrFailAsync<DiscountSettings, Guid>(id);

        public async Task<DiscountSettings> CreateAsync(DiscountSettings entity)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await discountSettingsValidator.ValidateAsync(entity, currentUser.CompanyId);
            return await unitOfWork.AddAndSaveAsync(entity);
        }

        public async Task<DiscountSettings> UpdateAsync(DiscountSettings entity)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await discountSettingsValidator.ValidateAsync(entity, currentUser.CompanyId);
            return await unitOfWork.UpdateAndSaveAsync<DiscountSettings, Guid>(entity);
        }
    }
}