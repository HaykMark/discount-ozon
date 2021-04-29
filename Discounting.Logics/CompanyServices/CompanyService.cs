using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.TariffDiscounting;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.CompanyServices
{
    public interface ICompanyService
    {
        Task<(Company[], int)> GetAll(int offset, int limit, string tin, CompanyType companyType);

        Task<Company> Get(Guid id);
        Task<Company> GetDetailedAsync(Guid id);

        Task<(User[], int)> GetUsers(Guid id, int offset, int limit);
        Task<(Contract[], int)> GetContracts(Guid id, int offset, int limit);
        Task<(Tariff[], int)> GetTariffs(Guid id, int offset, int limit);
        Task<Company> CreateAsync(Company company);
        Task<Company> UpdateAsync(Company company);
        Task DeactivateAsync(Guid id, string deactivationReason);
        Task ActivateAsync(Guid id);
    }

    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;
        private readonly ICompanyValidator companyValidator;

        public CompanyService(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            ICompanyValidator companyValidator
        )
        {
            this.unitOfWork = unitOfWork;
            this.sessionService = sessionService;
            this.companyValidator = companyValidator;
        }

        public async Task<(Company[], int)> GetAll(int offset, int limit, string tin, CompanyType companyType)
        {
            var query = GetBaseQuery(tin, companyType);
            return (await GetBaseQuery(tin, companyType)
                    .OrderBy(c => c.ShortName)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        private IQueryable<Company> GetBaseQuery(string tin, CompanyType companyType)
        {
            return unitOfWork.Set<Company>()
                .OrderBy(c => c.ShortName)
                .Where(c => c.CompanyType == companyType &&
                            (string.IsNullOrEmpty(tin) ||
                             c.TIN == tin));
        }

        public Task<Company> Get(Guid id) =>
            unitOfWork.GetOrFailAsync<Company, Guid>(id);

        public Task<Company> GetDetailedAsync(Guid id) =>
            unitOfWork.GetOrFailAsync(id, unitOfWork.Set<Company>()
                .Include(c => c.Users));

        public async Task<Company> CreateAsync(Company company)
        {
            company.IsActive = true;
            return await unitOfWork.AddAndSaveAsync(company);
        }

        public async Task<Company> UpdateAsync(Company company)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync(userId, unitOfWork.Set<User>().Include(u => u.Company));
            company.IsActive = user.Company.IsActive;
            await companyValidator.ValidateAsync(company, user);
            return await unitOfWork.UpdateAndSaveAsync<Company, Guid>(company);
        }

        public async Task DeactivateAsync(Guid id, string deactivationReason)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            var company = await unitOfWork.GetOrFailAsync<Company, Guid>(id);
            companyValidator.ValidateDeactivation(company, user);
            company.DeactivationReason = deactivationReason;
            company.IsActive = false;
            await unitOfWork.UpdateAndSaveAsync<Company, Guid>(company);
        }

        public async Task ActivateAsync(Guid id)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            var company = await unitOfWork.GetOrFailAsync<Company, Guid>(id);
            companyValidator.ValidateActivation(company, user);
            company.IsActive = true;
            await unitOfWork.UpdateAndSaveAsync<Company, Guid>(company);
        }

        public async Task<(User[], int)> GetUsers(Guid id, int offset, int limit)
        {
            var query = unitOfWork
                .Set<Company>()
                .Include(c => c.Users)
                .Where(c => c.Id == id)
                .SelectMany(c => c.Users);
            return (await query
                    .OrderByDescending(u => u.FirstName)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<(Contract[], int)> GetContracts(Guid id, int offset, int limit)
        {
            var query = unitOfWork
                .Set<Contract>()
                .Where(c => c.SellerId == id ||
                            c.BuyerId == id);
            return (await query
                    .OrderByDescending(u => u.CreationDate)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<(Tariff[], int)> GetTariffs(Guid id, int offset, int limit)
        {
            var query = unitOfWork
                .Set<Tariff>()
                .Where(c => c.User.CompanyId == id);
            return (await query
                    .OrderBy(u => u.FromDay)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }
    }
}