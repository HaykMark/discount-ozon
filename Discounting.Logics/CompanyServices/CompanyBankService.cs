using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.Account;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.CompanyServices
{
    public interface ICompanyBankService
    {
        Task<CompanyBankInfo[]> GetAllAsync(bool? isActive = null);
        Task<CompanyBankInfo> GetAsync(Guid companyId);
        Task<CompanyBankInfo> CreateOrUpdateAsync(CompanyBankInfo companyBankInfo);
    }

    public class CompanyBankService : ICompanyBankService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;

        public CompanyBankService(IUnitOfWork unitOfWork,  ISessionService sessionService)
        {
            this.unitOfWork = unitOfWork;
            this.sessionService = sessionService;
        }

        public Task<CompanyBankInfo[]> GetAllAsync(bool? isActive = null)
        {
            return unitOfWork.Set<CompanyBankInfo>()
                .Where(c => !isActive.HasValue || c.IsActive == isActive)
                .ToArrayAsync();
        }

        public async Task<CompanyBankInfo> GetAsync(Guid companyId)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.Set<User>()
                .Where(u => u.Id == currentUserId)
                .FirstAsync();
            if (!user.IsSuperAdmin && companyId != user.CompanyId)
            {
                throw new ForbiddenException();
            }

            var companyBankInfo = await unitOfWork.Set<CompanyBankInfo>()
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .FirstOrDefaultAsync();
            if (companyBankInfo == null)
            {
                throw new NotFoundException();
            }

            return companyBankInfo;
        }

        public async Task<CompanyBankInfo> CreateOrUpdateAsync(CompanyBankInfo companyBankInfo)
        {
            companyBankInfo.Id = Guid.NewGuid();
            companyBankInfo.IsActive = true;
            companyBankInfo.UserId = sessionService.GetCurrentUserId();
            //Update previews: set IsActive to false and add a new Entry
            if (await unitOfWork.Set<CompanyBankInfo>().AnyAsync(c => c.CompanyId == companyBankInfo.CompanyId))
            {
                var previewsActiveBankInfo = await unitOfWork.Set<CompanyBankInfo>()
                    .FirstOrDefaultAsync(c => c.CompanyId == companyBankInfo.CompanyId && c.IsActive);
                previewsActiveBankInfo.IsActive = false;
                unitOfWork.Set<CompanyBankInfo>().Update(previewsActiveBankInfo);
            }
            
            return await unitOfWork.AddAndSaveAsync(companyBankInfo);
        }
    }
}