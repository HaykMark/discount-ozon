using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics
{
    public interface IContractService
    {
        Task<(Contract[], int)> GetAllAsync(int offset, int limit);
        Task<Contract> GetAsync(Guid id);
        Task<Contract> GetDetailedAsync(Guid id);
        Task<Contract> CreateAsync(Contract contract);
        Task<Contract> UpdateAsync(Contract contract);
        Task InitAsync(Contract contract, Guid currentUserId);
    }

    public class ContractService : IContractService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IContractValidator contractValidator;
        private readonly ISessionService sessionService;

        public ContractService(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            IContractValidator contractValidator
        )
        {
            this.unitOfWork = unitOfWork;
            this.sessionService = sessionService;
            this.contractValidator = contractValidator;
        }

        private IQueryable<Contract> GetBaseQuery()
        {
            return unitOfWork
                .Set<Contract>()
                .Include(c => c.Seller);
        }

        public async Task<(Contract[], int)> GetAllAsync(int offset, int limit)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            var query = GetBaseQuery()
                .Where(c => currentUser.IsSuperAdmin ||
                            c.SellerId == currentUser.CompanyId ||
                            c.BuyerId == currentUser.CompanyId);
            return (await query
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<Contract> GetAsync(Guid id)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            await contractValidator.ValidateRequestedContractPermission(id, currentUserId);
            return await unitOfWork.GetOrFailAsync(id, GetBaseQuery());
        }

        public async Task<Contract> GetDetailedAsync(Guid id)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            await contractValidator.ValidateRequestedContractPermission(id, currentUserId);
            var contract = await unitOfWork.GetOrFailAsync(id,
                unitOfWork
                    .Set<Contract>()
                    .Include(c => c.Seller)
                    .ThenInclude(c => c.Users)
                    .Include(c => c.Buyer));
            if (!contract.Seller.Users.Any())
            {
                throw new NotFoundException("Sellers");
            }

            return contract;
        }

        public async Task<Contract> CreateAsync(Contract contract)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            contract.Status = ContractStatus.Active;
            contract.Provider = ContractProvider.Manually;
            await InitAsync(contract, currentUserId);
            await contractValidator.ValidateAsync(contract, currentUserId);
            await unitOfWork.AddAndSaveAsync(contract);
            return await unitOfWork.GetOrFailAsync(contract.Id, GetBaseQuery());
        }

        public async Task<Contract> UpdateAsync(Contract contract)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            await InitAsync(contract, currentUserId);
            contract.UpdateDate = DateTime.UtcNow;
            await contractValidator.ValidateAsync(contract, currentUserId);
            await unitOfWork.UpdateAndSaveAsync<Contract, Guid>(contract);
            return await unitOfWork.GetOrFailAsync(contract.Id, GetBaseQuery());
        }

        public async Task InitAsync(Contract contract, Guid currentUserId)
        {
            var user = await unitOfWork.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            contract.BuyerId = user.CompanyId;
            contract.Buyer = null;
            contract.CreationDate = DateTime.UtcNow;
            contract.CreatorId = currentUserId;
            contract.Creator = null;
            var seller = await unitOfWork.Set<Company>()
                .FirstOrDefaultAsync(c => c.TIN == contract.Seller.TIN);
            if (seller != null)
            {
                contract.SellerId = seller.Id;
                contract.Seller = null;
            }
        }
    }
}