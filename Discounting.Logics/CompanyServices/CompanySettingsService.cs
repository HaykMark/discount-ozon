using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.CompanyServices
{
    public interface ICompanySettingsService
    {
        Task<CompanySettings> GetSettings(Guid id);
        Task<CompanySettings> CreateSettingsAsync(CompanySettings settings);
        Task<CompanySettings> UpdateSettingsAsync(CompanySettings settings);
    }

    public class 
        CompanySettingsService : ICompanySettingsService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;
        private readonly ICompanyValidator companyValidator;

        public CompanySettingsService(
            ICompanyValidator companyValidator,
            ISessionService sessionService,
            IUnitOfWork unitOfWork
        )
        {
            this.companyValidator = companyValidator;
            this.sessionService = sessionService;
            this.unitOfWork = unitOfWork;
        }

        public async Task<CompanySettings> GetSettings(Guid id)
        {
            var settings = await unitOfWork.Set<CompanySettings>()
                .FirstOrDefaultAsync(c => c.CompanyId == id);
            if (settings == null)
            {
                var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(sessionService.GetCurrentUserId());
                settings = new CompanySettings
                {
                    UserId = currentUser.Id,
                    CompanyId = currentUser.CompanyId,
                    CreationDate = DateTime.UtcNow,
                    IsAuction = false,
                    ForbidSellerEditTariff = false,
                    IsSendAutomatically = false
                };
                return await unitOfWork.AddAndSaveAsync(settings);
            }

            return settings;
        }

        public async Task<CompanySettings> CreateSettingsAsync(CompanySettings settings)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            await companyValidator.ValidateSettingsAsync(settings, user);
            settings.UserId = user.Id;
            settings.CompanyId = user.CompanyId;
            settings.Company = null;
            return await unitOfWork.AddAndSaveAsync(settings);
        }

        public async Task<CompanySettings> UpdateSettingsAsync(CompanySettings settings)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            await companyValidator.ValidateSettingsAsync(settings, user);
            return await unitOfWork.UpdateAndSaveAsync<CompanySettings, Guid>(settings);
        }
    }
}