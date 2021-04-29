using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.FactoringAgreements
{
    public interface IFactoringAgreementService
    {
        Task<(List<FactoringAgreement>, int)> Get(
            Guid? companyId,
            Guid? bankId,
            string supplyNumber,
            int offset,
            int limit
        );

        Task<FactoringAgreement> Get(Guid id);
        Task<FactoringAgreement> GetDetailedAsync(Guid id);
        Task<FactoringAgreement> CreateAsync(FactoringAgreement factoringAgreement);
        Task<FactoringAgreement> UpdateAsync(FactoringAgreement factoringAgreement);
    }

    public class FactoringAgreementService : IFactoringAgreementService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;
        private readonly IFactoringAgreementValidator factoringAgreementValidator;

        public FactoringAgreementService(
            IFactoringAgreementValidator factoringAgreementValidator,
            ISessionService sessionService,
            IUnitOfWork unitOfWork
        )
        {
            this.factoringAgreementValidator = factoringAgreementValidator;
            this.sessionService = sessionService;
            this.unitOfWork = unitOfWork;
        }

        public async Task<(List<FactoringAgreement>, int)> Get(
            Guid? companyId,
            Guid? bankId,
            string supplyNumber,
            int offset,
            int limit
        )
        {
            var query = unitOfWork.Set<FactoringAgreement>()
                .Include(s => s.SupplyFactoringAgreements)
                .Where(c => (!companyId.HasValue || c.CompanyId == companyId) &&
                            (!bankId.HasValue || c.BankId == bankId) &&
                            (string.IsNullOrEmpty(supplyNumber) ||
                             (c.IsActive &&
                              c.SupplyFactoringAgreements
                                  .Any(s => s.Number == supplyNumber))));
            return (await query
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync(),
                await query.CountAsync());
        }

        public Task<FactoringAgreement> Get(Guid id)
        {
            return unitOfWork.GetOrFailAsync(id,
                unitOfWork.Set<FactoringAgreement>()
                    .Include(f => f.SupplyFactoringAgreements));
        }

        public Task<FactoringAgreement> GetDetailedAsync(Guid id)
        {
            return unitOfWork.GetOrFailAsync(id,
                unitOfWork.Set<FactoringAgreement>()
                    .Include(f => f.Company)
                    .ThenInclude(c => c.Users)
                    .Include(f => f.Bank)
                    .ThenInclude(b => b.Users)
            );
        }

        public async Task<FactoringAgreement> CreateAsync(FactoringAgreement factoringAgreement)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            factoringAgreement.CompanyId = user.CompanyId;
            await factoringAgreementValidator.ValidateAsync(factoringAgreement, user);
            await InitAsync(factoringAgreement);

            return await unitOfWork.AddAndSaveAsync(factoringAgreement);
        }

        public async Task<FactoringAgreement> UpdateAsync(FactoringAgreement factoringAgreement)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            await factoringAgreementValidator.ValidateAsync(factoringAgreement, user);
            await InitAsync(factoringAgreement);

            return await unitOfWork.UpdateAndSaveAsync<FactoringAgreement, Guid>(factoringAgreement);
        }

        private async Task ConfirmAsync(FactoringAgreement entity)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync(userId, unitOfWork.Set<User>().Include(u => u.Company));
            factoringAgreementValidator.ValidateBankConfirmation(entity, user);
            entity.IsActive = true;
            entity.IsConfirmed = true;

            entity.SupplyFactoringAgreements.ForEach(f => f.Status = SupplyFactoringAgreementStatus.Active);
        }

        private async Task InitAsync(FactoringAgreement factoringAgreement)
        {
            if (factoringAgreement.Id == default)
            {
                factoringAgreement.CreationDate = DateTime.UtcNow;
                factoringAgreement.IsConfirmed = false;
                factoringAgreement.IsActive = false;
                if (factoringAgreement.SupplyFactoringAgreements.Any())
                {
                    factoringAgreement.SupplyFactoringAgreements.ForEach(s =>
                        s.Status = SupplyFactoringAgreementStatus.NotActive);
                }
            }
            else if (factoringAgreement.IsConfirmed &&
                     await unitOfWork.Set<FactoringAgreement>()
                         .AnyAsync(f => f.Id == factoringAgreement.Id &&
                                        !f.IsConfirmed))
            {
                await ConfirmAsync(factoringAgreement);
            }
        }
    }
}