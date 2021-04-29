using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.TariffDiscounting;
using Discounting.Entities.Templates;
using Discounting.Helpers;
using Discounting.Logics.Account;
using Discounting.Logics.Excel;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics
{
    public interface IRegistryService
    {
        Task<Registry> GetDetailedAsync(Guid id);
        Task<(Registry[], int)> GetAsync(int offset, int limit, RegistryStatus status);
        Task<Registry> CreateAsync(Guid[] supplyIds, FinanceType financeType, Guid? bankId, Guid? factoringAgreementId);
        Task<Registry> UpdateAsync(Registry registry);
        Task RemoveAsync(Guid id);
        Task<Registry> SetSuppliesAsync(Guid registryId, Guid[] supplyIds);
        Task<(Supply[], int)> GetRegistrySuppliesAsync(Guid id, int offset, int limit);
        Task<string> GetRegistryFileAsync(Guid id, TemplateType type);
        Task<RegistrySignature[]> GetRegistrySignaturesAsync(Guid id);
        Task<ZipItem[]> GetZipItemsAsync(Guid id);
        Task<List<string>> GetRegistryFilesPath(Guid id);
    }

    public class RegistryService : IRegistryService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IRegistryValidator registryValidator;
        private readonly ISessionService sessionService;
        private readonly IExcelDocumentGeneratorService excelDocumentGeneratorService;
        private readonly ISignatureService signatureService;

        public RegistryService(
            IUnitOfWork unitOfWork,
            IRegistryValidator registryValidator,
            ISessionService sessionService,
            IExcelDocumentGeneratorService excelDocumentGeneratorService,
            ISignatureService signatureService
        )
        {
            this.unitOfWork = unitOfWork;
            this.registryValidator = registryValidator;
            this.sessionService = sessionService;
            this.excelDocumentGeneratorService = excelDocumentGeneratorService;
            this.signatureService = signatureService;
        }

        public async Task<Registry> GetDetailedAsync(Guid id)
        {
            return await unitOfWork.GetOrFailAsync(id, unitOfWork.Set<Registry>()
                .Include(r => r.Bank)
                .ThenInclude(r => r.Users)
                .Include(r => r.Contract)
                .ThenInclude(c => c.Seller)
                .ThenInclude(c => c.Users)
                .Include(r => r.Contract)
                .ThenInclude(c => c.Buyer)
                .ThenInclude(c => c.Users));
        }

        public async Task<(Registry[], int)> GetAsync(int offset, int limit, RegistryStatus status)
        {
            var query = await GetBaseGetQueryAsync(status);
            return (await query
                    .OrderByDescending(u => u.Date)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<RegistrySignature[]> GetRegistrySignaturesAsync(Guid id)
        {
            var signatures = await signatureService.TryGetAsync(SignatureType.Registry, id);
            return signatures.OfType<RegistrySignature>().ToArray();
        }

        public async Task<Registry> CreateAsync(Guid[] supplyIds, FinanceType financeType, Guid? bankId,
            Guid? factoringAgreementId)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var supplies = await unitOfWork.Set<Supply>()
                .Include(s => s.Contract)
                .Where(s => supplyIds.Contains(s.Id))
                .ToArrayAsync();
            var user = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .ThenInclude(c => c.FactoringAgreements)
                .ThenInclude(c => c.SupplyFactoringAgreements)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            registryValidator.ValidateSupplies(supplies, financeType, user, bankId, factoringAgreementId);
            var registry = await InitRegistryAsync(supplies, financeType, user, bankId, factoringAgreementId);
            return await unitOfWork.AddAndSaveAsync(registry);
        }

        public async Task<Registry> UpdateAsync(Registry registry)
        {
            if (registry.Status == RegistryStatus.Declined)
            {
                return await DeclineAsync(registry);
            }

            var entity = await unitOfWork.GetOrFailAsync(registry.Id,
                unitOfWork
                    .Set<Registry>()
                    .Include(r => r.Contract));

            await InitUpdateRegistryAsync(registry, entity);
            return await unitOfWork.UpdateAndSaveAsync<Registry, Guid>(entity);
        }

        public async Task RemoveAsync(Guid id)
        {
            if (await unitOfWork.Set<Registry>()
                .AnyAsync(r => r.Id == id &&
                               (r.Status != RegistryStatus.InProcess ||
                                r.IsConfirmed ||
                                r.IsVerified)))
            {
                throw new ForbiddenException();
            }

            var supplies = await unitOfWork.Set<Supply>()
                .Include(s => s.SupplyDiscount)
                .Where(s => s.RegistryId == id)
                .ToListAsync();
            supplies.ForEach(s => { s.Status = SupplyStatus.InProcess; });
            var supplyDiscount = supplies
                .Where(s => s.SupplyDiscount != null)
                .Select(s => s.SupplyDiscount).ToList();
            if (supplyDiscount.Any())
            {
                unitOfWork.Set<SupplyDiscount>().RemoveRange(supplyDiscount);
            }

            unitOfWork.Set<Supply>().UpdateRange(supplies);
            await unitOfWork.RemoveAndSaveAsync<Registry, Guid>(id);
        }

        public async Task<Registry> SetSuppliesAsync(Guid registryId, Guid[] supplyIds)
        {
            var entity = await unitOfWork.GetOrFailAsync(registryId,
                unitOfWork.Set<Registry>()
                    .Include(r => r.Discount)
                    .Include(r => r.Contract)
                    .Include(r => r.Supplies)
                    .ThenInclude(s => s.SupplyDiscount));

            registryValidator.ValidateRegistrySupplyChangeAsync(entity, supplyIds);

            var existingSupplies = entity.Supplies.Where(s => supplyIds.Contains(s.Id)).ToList();
            var suppliesToDelete = entity.Supplies.Except(existingSupplies).ToList();
            if (!suppliesToDelete.Any())
            {
                return entity;
            }

            if (entity.SignStatus != RegistrySignStatus.NotSigned)
            {
                entity.SignStatus = RegistrySignStatus.NotSigned;
                await signatureService.RemoveIfAnyAsync(SignatureType.Registry, entity.Id);
            }

            RemoveSuppliesFromRegistry(suppliesToDelete);
            entity.Supplies = existingSupplies;
            entity.Amount = entity.Supplies
                .Where(s => s.Type != SupplyType.Invoice)
                .Sum(s => s.Amount);
            if (entity.FinanceType == FinanceType.DynamicDiscounting)
            {
                RecalculateDiscount(entity);
            }

            unitOfWork.Set<Registry>().Update(entity);
            await unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<(Supply[], int)> GetRegistrySuppliesAsync(Guid id, int offset, int limit)
        {
            var query = unitOfWork.Set<Supply>()
                .Where(s => s.RegistryId == id);
            return (await query
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<string> GetRegistryFileAsync(Guid id, TemplateType type)
        {
            var registry = await GetRegistryWithIncludes(id);
            await registryValidator.ValidateTemplateAsync(registry, type);
            return await excelDocumentGeneratorService.GetOrGenerateRegistryTemplateAsync(registry, type);
        }

        public async Task<ZipItem[]> GetZipItemsAsync(Guid id)
        {
            var registryFilesPath = await GetRegistryFilesPath(id);

            return registryFilesPath.Select(path =>
                    new ZipItem
                    {
                        Path = path,
                        Name = Path.GetFileName(path)
                    })
                .ToArray();
        }

        public async Task<List<string>> GetRegistryFilesPath(Guid id)
        {
            var paths = new List<string>();
            var registry = await GetRegistryWithIncludes(id);
            var templateTypes = new List<TemplateType>();
            if (registry.FinanceType == FinanceType.DynamicDiscounting)
            {
                templateTypes.Add(TemplateType.Discount);
            }
            else
            {
                if (registry.Contract.IsRequiredRegistry)
                {
                    templateTypes.Add(TemplateType.Registry);
                }

                if (registry.Contract.IsRequiredNotification)
                {
                    templateTypes.Add(TemplateType.Verification);
                }
            }

            foreach (var templateType in templateTypes)
            {
                var path =
                    await excelDocumentGeneratorService.GetOrGenerateRegistryTemplateAsync(registry, templateType);
                paths.Add(path);
            }

            return paths;
        }

        private async Task<Registry> DeclineAsync(Registry registry)
        {
            var entity = await unitOfWork.GetOrFailAsync(registry.Id,
                unitOfWork.Set<Registry>()
                    .Include(r => r.Contract)
                    .Include(r => r.Supplies)
                    .ThenInclude(s => s.SupplyDiscount));
            var currentUserId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            registryValidator.ValidateBeforeDeclineAsync(entity, user);
            RemoveSuppliesFromRegistry(entity.Supplies.ToList());
            entity.Status = RegistryStatus.Declined;
            entity.SignStatus = RegistrySignStatus.NotSigned;
            entity.Supplies = null;

            entity.Remark = registry.Remark;
            unitOfWork.Set<Registry>().Update(entity);
            await unitOfWork.SaveChangesAsync();
            await signatureService.RemoveIfAnyAsync(SignatureType.Registry, registry.Id);
            return entity;
        }

        private async Task<Registry> InitRegistryAsync(
            Supply[] supplies,
            FinanceType financeType,
            User currentUser,
            Guid? bankId,
            Guid? factoringAgreementId
        )
        {
            var registryId = Guid.NewGuid();
            var contract = supplies.First().Contract;
            var lastNumber = (await unitOfWork.Set<Registry>()
                .Where(r =>
                    r.Contract.SellerId == contract.SellerId &&
                    r.Status != RegistryStatus.Declined &&
                    r.FinanceType == financeType &&
                    r.Date.Year == DateTime.Now.Year)
                .OrderByDescending(r => r.Id)
                .MaxAsync(r => (int?) r.Number)) ?? 0;
            foreach (var supply in supplies)
            {
                supply.Contract = null;
                supply.RegistryId = registryId;
                supply.BankId = bankId;
                supply.Status = SupplyStatus.InFinance;
            }

            unitOfWork.Set<Supply>().UpdateRange(supplies);
            var registry = new Registry
            {
                Id = registryId,
                Number = lastNumber + 1,
                Amount = supplies.Where(s => s.Type != SupplyType.Invoice).Sum(s => s.Amount),
                Status = RegistryStatus.InProcess,
                SignStatus = RegistrySignStatus.NotSigned,
                BankId = bankId,
                ContractId = contract.Id,
                Date = DateTime.UtcNow,
                CreatorId = currentUser.CompanyId,
                FinanceType = financeType,
                FactoringAgreementId = factoringAgreementId
            };

            return registry;
        }

        private async Task InitUpdateRegistryAsync(Registry registry, Registry entity)
        {
            entity.SignStatus = registry.SignStatus;
            if (registry.BankId.HasValue &&
                (!entity.BankId.HasValue ||
                 registry.BankId.Value != entity.BankId.Value))
            {
                var currentUserId = sessionService.GetCurrentUserId();
                await registryValidator.ValidateBankAsync(registry.BankId.Value, currentUserId);
                entity.BankId = registry.BankId.Value;
            }
        }

        private void RemoveSuppliesFromRegistry(List<Supply> supplies)
        {
            foreach (var supply in supplies)
            {
                supply.Status = supply.DelayEndDate <= DateTime.UtcNow
                    ? SupplyStatus.NotAvailable
                    : SupplyStatus.InProcess;
                supply.Registry = null;
                supply.RegistryId = null;
                supply.BankId = null;
                //supply.SupplyDiscount = null;
            }

            var supplyDiscount = supplies
                .Where(s => s.SupplyDiscount != null)
                .Select(s => s.SupplyDiscount).ToList();
            if (supplyDiscount.Any())
            {
                unitOfWork.Set<SupplyDiscount>().RemoveRange(supplyDiscount);
            }

            unitOfWork.Set<Supply>().UpdateRange(supplies);
        }

        private async Task<IQueryable<Registry>> GetBaseGetQueryAsync(RegistryStatus status)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser.IsSuperAdmin)
            {
                return unitOfWork
                    .Set<Registry>()
                    .Where(r => r.Status == status);
            }

            if (currentUser.Company.CompanyType == CompanyType.SellerBuyer)
            {
                return unitOfWork
                    .Set<Registry>()
                    .Where(r => r.Status == status &&
                                (r.Contract.SellerId == currentUser.CompanyId ||
                                 (r.Contract.BuyerId == currentUser.CompanyId &&
                                  (r.SignStatus != RegistrySignStatus.NotSigned || r.IsConfirmed))));
            }

            return unitOfWork
                .Set<Registry>()
                .Where(s => s.BankId.HasValue &&
                            s.BankId.Value == currentUser.CompanyId &&
                            s.Status == status &&
                            (s.SignStatus == RegistrySignStatus.SignedBySellerBuyer ||
                             s.SignStatus == RegistrySignStatus.SignedByAll));
        }

        private void RecalculateDiscount(Registry registry)
        {
            var discount = registry.Discount;
            if (discount is null)
            {
                throw new NotFoundException(typeof(Discount));
            }

            if (!discount.HasChanged)
            {
                //Done with AsEnumerable because ef core cannot translate this expression
                var buyerTariffs = unitOfWork.Set<Tariff>()
                    .Where(t => t.User.CompanyId == registry.Contract.BuyerId)
                    .AsEnumerable()
                    .Where(t => t.FromAmount <= registry.Amount &&
                                (!t.UntilAmount.HasValue ||
                                 t.UntilAmount.Value >= registry.Amount)
                    )
                    .ToList();

                var daysInThisYear = DateTime.IsLeapYear(DateTime.UtcNow.Year) ? 366 : 365;
                foreach (var supply in registry.Supplies.Where(s => Supply.IsMainType(s.Type)))
                {
                    var currentTariff = buyerTariffs.FirstOrDefault(t =>
                        t.FromDay <= (supply.DelayEndDate - discount.PlannedPaymentDate).Days &&
                        (!t.UntilDay.HasValue ||
                         t.UntilDay.Value >= (supply.DelayEndDate - discount.PlannedPaymentDate).Days));

                    if (currentTariff != null && supply.SupplyDiscount != null)
                    {
                        supply.SupplyDiscount.Rate = currentTariff.Rate;
                        supply.SupplyDiscount.DiscountedAmount = currentTariff.Type == TariffType.Discounting
                            ? supply.Amount * currentTariff.Rate * 0.01M
                            : (currentTariff.Rate / daysInThisYear) *
                              (supply.DelayEndDate.Date - discount.PlannedPaymentDate.Date).Days *
                              supply.Amount * 0.01M;
                    }
                }
            }

            unitOfWork.Set<SupplyDiscount>()
                .UpdateRange(registry.Supplies.Where(s => s.SupplyDiscount != null).Select(s => s.SupplyDiscount));

            var mainSuppliesWithDiscount = registry.Supplies
                .Where(s => Supply.IsMainType(s.Type) && s.SupplyDiscount != null)
                .ToList();
            if (mainSuppliesWithDiscount.Any())
            {
                discount.DiscountedAmount = mainSuppliesWithDiscount.Sum(e => e.SupplyDiscount.DiscountedAmount);
            }

            discount.AmountToPay = registry.Amount - discount.DiscountedAmount;
            discount.Rate = (registry.Amount - discount.AmountToPay) / registry.Amount * 100;

            unitOfWork.Set<Discount>().Update(discount);
        }

        private async Task<Registry> GetRegistryWithIncludes(Guid id)
        {
            var registry = await unitOfWork.GetOrFailAsync(id,
                unitOfWork.Set<Registry>()
                    .Include(r => r.Discount)
                    .Include(r => r.Contract)
                    .Include(c => c.Contract.Buyer)
                    .Include(c => c.Contract.Buyer.CompanyContactInfo)
                    .Include(c => c.Contract.Buyer.CompanyOwnerPositionInfo)
                    .Include(c => c.Contract.Seller)
                    .Include(c => c.Contract.Seller.CompanyContactInfo)
                    .Include(c => c.Contract.Seller.CompanyOwnerPositionInfo)
                    .Include(r => r.Supplies)
                        .ThenInclude(s => s.SupplyDiscount)
                    .Include(r => r.Bank)
                    .Include(x => x.FactoringAgreement));
            var sellerBankInfo = await unitOfWork.Set<CompanyBankInfo>()
                .SingleOrDefaultAsync(c => c.IsActive && c.CompanyId == registry.Contract.SellerId);
            var buyerBankInfo = await unitOfWork.Set<CompanyBankInfo>()
                .SingleOrDefaultAsync(c => c.IsActive && c.CompanyId == registry.Contract.BuyerId);
            if (sellerBankInfo != null)
            {
                registry.Contract.Seller.CompanyBankInfos.Add(sellerBankInfo);
            }

            if (buyerBankInfo != null)
            {
                registry.Contract.Buyer.CompanyBankInfos.Add(buyerBankInfo);
            }

            if (registry.BankId.HasValue)
            {
                var factorBankInfo = await unitOfWork.Set<CompanyBankInfo>()
                    .SingleOrDefaultAsync(c => c.IsActive && c.CompanyId == registry.BankId);
                if (factorBankInfo != null)
                {
                    registry.Bank.CompanyBankInfos.Add(factorBankInfo);
                }
            }

            return registry;
        }
    }
}