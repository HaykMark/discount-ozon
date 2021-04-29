using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.TariffDiscounting;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics
{
    public interface ISupplyService
    {
        Task<(Supply[], int)> GetInProcess(int offset, int limit);
        Task<(Supply[], int)> GetInFinance(int offset, int limit);
        Task<(Supply[], int)> GetNotAvailable(int offset, int limit);
        Task<Supply> Get(Guid id);
        Task<SupplyDiscount> GetDiscount(Guid id);
        Task<(List<Supply>, ValidationErrors)> CreateAsync(List<Supply> supply, SupplyProvider provider);
        Task<(List<Supply>, ValidationErrors)> VerifySellerManualAsync(Guid[] supplyIds, Guid bankId, Guid factoringAgreementId);
        Task<(List<Supply>, ValidationErrors)> VerifyBuyerManualAsync(Guid[] supplyIds);
        Task<(List<Supply>, ValidationErrors)> VerifyAutomaticallyAsync(Guid[] supplyIds);
        Task UpdateNotAvailableSupplies();
    }

    public class SupplyService : ISupplyService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISupplyValidator supplyValidator;
        private readonly ISessionService sessionService;
        private readonly IContractService contractService;

        public SupplyService(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            ISupplyValidator supplyValidator,
            IContractService contractService
        )
        {
            this.unitOfWork = unitOfWork;
            this.sessionService = sessionService;
            this.supplyValidator = supplyValidator;
            this.contractService = contractService;
        }

        private async Task<IQueryable<Supply>> GetBaseQuery(User currentUser, SupplyStatus status)
        {
            var allContractIds = await unitOfWork
                .Set<Contract>()
                .Where(c => currentUser.IsSuperAdmin ||
                            c.SellerId == currentUser.CompanyId ||
                            c.BuyerId == currentUser.CompanyId)
                .Select(c => c.Id)
                .ToListAsync();
            return unitOfWork
                .Set<Supply>()
                .Include(s => s.Contract)
                .ThenInclude(c => c.Seller)
                .Include(s => s.Contract)
                .ThenInclude(c => c.Buyer)
                .Where(s => s.Status == status &&
                            allContractIds.Contains(s.ContractId));
        }

        public async Task<(Supply[], int)> GetInProcess(int offset, int limit)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.Set<User>()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            IQueryable<Supply> query;
            if (currentUser.IsSuperAdmin)
            {
                query = await GetBaseQuery(currentUser, SupplyStatus.InProcess);
            }
            else
            {
                query = (await GetBaseQuery(currentUser, SupplyStatus.InProcess))
                    .Where(s => s.Contract.SellerId == currentUser.CompanyId ||
                                (s.Contract.BuyerId == currentUser.CompanyId &&
                                 (s.SellerVerified ||
                                  !s.AddedBySeller ||
                                  s.RegistryId.HasValue)));
            }

            return (await query
                    .OrderByDescending(u => u.CreationDate)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<(Supply[], int)> GetInFinance(int offset, int limit)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user.Company.CompanyType == CompanyType.SellerBuyer ||
                user.IsSuperAdmin)
            {
                var query = await GetBaseQuery(user, SupplyStatus.InFinance);
                return (await query
                        .OrderByDescending(u => u.UpdateDate)
                        .Skip(offset)
                        .Take(limit)
                        .ToArrayAsync(),
                    await query.CountAsync());
            }

            var bankQuery = unitOfWork
                .Set<Supply>()
                .Include(s => s.Contract)
                .ThenInclude(c => c.Seller)
                .Include(s => s.Contract)
                .ThenInclude(c => c.Buyer)
                .Where(s =>
                    (
                        (s.Status == SupplyStatus.InFinance && !s.RegistryId.HasValue) ||
                        (s.Status == SupplyStatus.InFinance && s.RegistryId.HasValue &&
                         (s.Registry.SignStatus == RegistrySignStatus.SignedBySellerBuyer ||
                          s.Registry.SignStatus == RegistrySignStatus.SignedByAll))
                    ) &&
                    s.BankId.HasValue &&
                    s.BankId.Value == user.CompanyId);
            return (await bankQuery
                    .OrderByDescending(u => u.UpdateDate)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await bankQuery.CountAsync());
        }

        public async Task<(Supply[], int)> GetNotAvailable(int offset, int limit)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            var query = await GetBaseQuery(user, SupplyStatus.NotAvailable);
            return (await query
                    .OrderByDescending(u => u.UpdateDate)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<Supply> Get(Guid id)
        {
            return await unitOfWork.GetOrFailAsync<Supply, Guid>(id);
        }

        public async Task<SupplyDiscount> GetDiscount(Guid id)
        {
            var supplyDiscount = await unitOfWork.Set<SupplyDiscount>()
                .FirstOrDefaultAsync(s => s.SupplyId == id);
            if (supplyDiscount == null)
            {
                throw new NotFoundException(typeof(SupplyDiscount));
            }

            return supplyDiscount;
        }

        public async Task<(List<Supply>, ValidationErrors)> CreateAsync(List<Supply> supplies, SupplyProvider provider)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var errors = await supplyValidator.ValidateAsync(supplies, currentUserId);
            await InitAsync(supplies, currentUserId, provider);
            await unitOfWork.AddRangeAndSaveAsync(supplies);
            return (supplies, errors);
        }

        public async Task<(List<Supply>, ValidationErrors)> VerifySellerManualAsync(Guid[] supplyIds, Guid bankId, Guid factoringAgreementId)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var supplies = await unitOfWork.Set<Supply>()
                .Include(s => s.Contract)
                .ThenInclude(s => s.Seller)
                .Where(s => supplyIds.Contains(s.Id))
                .ToListAsync();
            var user = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .ThenInclude(c => c.FactoringAgreements)
                .Include(u => u.Company)
                .ThenInclude(c => c.CompanySettings)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            var errors = await supplyValidator.ValidateSellerManualVerificationAsync(supplies, bankId, user);
            if (supplies.Any())
            {
                supplies.ForEach(s =>
                {
                    s.SellerVerified = true;
                    s.Status = s.SellerVerified &&
                               s.BuyerVerified
                        ? SupplyStatus.InFinance
                        : SupplyStatus.InProcess;
                    s.UpdateDate = DateTime.UtcNow;
                    s.BankId = bankId;
                    s.FactoringAgreementId = factoringAgreementId;
                    s.Contract = null;
                    s.HasVerification = s.SellerVerified &&
                                        s.BuyerVerified;
                });

                unitOfWork.UpdateRange(supplies);
                await unitOfWork.SaveChangesAsync();
            }

            return (supplies, errors);
        }

        public async Task<(List<Supply>, ValidationErrors)> VerifyBuyerManualAsync(Guid[] supplyIds)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var supplies = await unitOfWork.Set<Supply>()
                .Include(s => s.Contract)
                .ThenInclude(s => s.Buyer)
                .Where(s => supplyIds.Contains(s.Id))
                .ToListAsync();
            var user = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            var errors = await supplyValidator.ValidateBuyerManualVerificationAsync(supplies, user);
            if (supplies.Any())
            {
                supplies.ForEach(s =>
                {
                    s.BuyerVerified = true;
                    s.Status = s.SellerVerified && s.BuyerVerified
                        ? SupplyStatus.InFinance
                        : SupplyStatus.InProcess;
                    s.UpdateDate = DateTime.UtcNow;
                    s.Contract = null;
                    s.HasVerification = s.SellerVerified &&
                                        s.BuyerVerified;
                });

                unitOfWork.UpdateRange(supplies);
                await unitOfWork.SaveChangesAsync();
            }

            return (supplies, errors);
        }

        public async Task<(List<Supply>, ValidationErrors)> VerifyAutomaticallyAsync(Guid[] supplyIds)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var supplies = await unitOfWork.Set<Supply>()
                .Include(s => s.Contract)
                .ThenInclude(s => s.Seller)
                .ThenInclude(s => s.CompanySettings)
                .Where(s => supplyIds.Contains(s.Id))
                .ToListAsync();
            var user = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .ThenInclude(c => c.FactoringAgreements)
                .Include(u => u.Company)
                .ThenInclude(u => u.CompanySettings)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            var errors = await supplyValidator.ValidateAutomaticallyVerificationAsync(supplies, user);
            if (supplies.Any())
            {
                supplies.ForEach(s =>
                {
                    s.Status = SupplyStatus.InFinance;
                    s.UpdateDate = DateTime.UtcNow;
                    s.Contract = null;
                    s.HasVerification = true;
                });

                unitOfWork.UpdateRange(supplies);
                await unitOfWork.SaveChangesAsync();
            }

            return (supplies, errors);
        }

        public async Task UpdateNotAvailableSupplies()
        {
            var supplies = await unitOfWork.Set<Supply>()
                .Where(s => s.DelayEndDate.Date <= DateTime.UtcNow.Date &&
                            s.Status == SupplyStatus.InProcess)
                .ToListAsync();
            if (supplies.Any())
            {
                supplies.ForEach(s =>
                {
                    s.Status = SupplyStatus.NotAvailable;
                    s.UpdateDate = DateTime.UtcNow;
                });
                unitOfWork.Set<Supply>().UpdateRange(supplies);
                await unitOfWork.SaveChangesAsync();
            }
        }

        private async Task InitAsync(List<Supply> supplies, Guid currentUserId, SupplyProvider provider)
        {
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            var createdContracts = new Dictionary<string, Contract>();
            foreach (var supply in supplies)
            {
                supply.Status = SupplyStatus.InProcess;
                supply.CreatorId = currentUserId;
                supply.Provider = provider;
                supply.BuyerVerified = true;

                //Init Contract
                if (supply.ContractId == default)
                {
                    var contract = await unitOfWork.Set<Contract>()
                        .FirstOrDefaultAsync(c => c.Seller.TIN == supply.Contract.Seller.TIN &&
                                                  c.Buyer.TIN == supply.Contract.Buyer.TIN);
                    if (contract != null)
                    {
                        if (contract.SellerId == user.CompanyId)
                        {
                            supply.AddedBySeller = true;
                        }

                        await InitSupplyStatus(supply, contract);
                        supply.ContractId = contract.Id;
                        supply.Contract = null;
                    }
                    else if (!createdContracts.ContainsKey(supply.Contract.Seller.TIN))
                    {
                        var sellerTin = supply.Contract.Seller.TIN;
                        await contractService.InitAsync(supply.Contract, currentUserId);
                        supply.Contract.Provider = ContractProvider.Automatically;
                        supply.Contract.Status = ContractStatus.Active;
                        supply.Contract.IsFactoring = true;
                        supply.ContractId = Guid.NewGuid();
                        supply.Contract.Id = supply.ContractId;
                        createdContracts.Add(sellerTin, supply.Contract);
                    }
                    else
                    {
                        supply.Contract = createdContracts[supply.Contract.Seller.TIN];
                        supply.ContractId = supply.Contract.Id;
                    }
                }

                //Init Supplies
                if (Supply.IsMainType(supply.Type))
                {
                    supply.SupplyId = Guid.NewGuid();
                    var childrenDb = await unitOfWork
                        .Set<Supply>()
                        .Where(s => s.BaseDocumentNumber == supply.Number &&
                                    s.BaseDocumentDate.HasValue &&
                                    s.BaseDocumentDate.Value.Date == supply.Date.Date &&
                                    s.BaseDocumentType == supply.Type)
                        .ToListAsync();
                    childrenDb.ForEach(c =>
                    {
                        c.BaseDocumentId = supply.Id;
                        c.SupplyId = supply.SupplyId;
                    });
                    var children = supplies
                        .Where(s => s.BaseDocumentNumber == supply.Number &&
                                    s.BaseDocumentDate.HasValue &&
                                    s.BaseDocumentDate.Value.Date == supply.Date.Date &&
                                    s.BaseDocumentType == supply.Type)
                        .ToList();
                    children.ForEach(c =>
                    {
                        c.BaseDocumentId = supply.Id;
                        c.SupplyId = supply.SupplyId;
                    });

                    unitOfWork.Set<Supply>().UpdateRange(childrenDb);
                }
                else if (!supply.BaseDocumentId.HasValue)
                {
                    var mainSupply = await unitOfWork.Set<Supply>()
                        .FirstOrDefaultAsync(s => s.Number == supply.BaseDocumentNumber &&
                                                  s.Date.Date == supply.BaseDocumentDate.Value.Date &&
                                                  s.Type == supply.BaseDocumentType);

                    if (mainSupply != null)
                    {
                        supply.BaseDocumentId = mainSupply.Id;
                        supply.SupplyId = mainSupply.SupplyId;
                    }
                }

                supply.BuyerVerified = !supply.AddedBySeller;
            }
        }

        private async Task InitSupplyStatus(Supply supply, Contract contract)
        {
            if (contract.IsFactoring)
            {
                var company = await unitOfWork.Set<Company>()
                    .Include(c => c.CompanySettings)
                    .Include(c => c.FactoringAgreements)
                    .FirstOrDefaultAsync(c => c.Id == contract.SellerId);
                if (!supply.AddedBySeller &&
                    company.CompanySettings != null &&
                    company.CompanySettings.IsSendAutomatically &&
                    company.FactoringAgreements.Any())
                {
                    supply.Status = contract.IsRequiredRegistry || contract.IsRequiredNotification
                        ? SupplyStatus.InProcess
                        : SupplyStatus.InFinance;
                    supply.HasVerification = !contract.IsRequiredNotification && !contract.IsRequiredNotification;
                    supply.UpdateDate = supply.Status == SupplyStatus.InFinance
                        ? DateTime.UtcNow
                        : (DateTime?) null;
                    supply.Bank = null;
                }
            }

            if (supply.DelayEndDate.Date <= DateTime.UtcNow.Date)
            {
                supply.Status = SupplyStatus.NotAvailable;
                supply.UpdateDate = DateTime.UtcNow;
            }
        }
    }
}