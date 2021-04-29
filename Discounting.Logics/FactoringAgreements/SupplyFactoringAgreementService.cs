using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.FactoringAgreements
{
    public interface ISupplyFactoringAgreementService
    {
        Task<SupplyFactoringAgreement> CreateAsync(SupplyFactoringAgreement entity);
        Task<SupplyFactoringAgreement> UpdateAsync(SupplyFactoringAgreement entity);
        Task<SupplyFactoringAgreement> Get(Guid id);
        Task<SupplyFactoringAgreement[]> GetAll(Guid? factoringAgreementId = null);
        Task RemoveAsync(Guid id);
    }

    public class SupplyFactoringAgreementService : ISupplyFactoringAgreementService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IFactoringAgreementValidator factoringAgreementValidator;

        public SupplyFactoringAgreementService(IUnitOfWork unitOfWork,
            IFactoringAgreementValidator factoringAgreementValidator)
        {
            this.unitOfWork = unitOfWork;
            this.factoringAgreementValidator = factoringAgreementValidator;
        }

        public async Task<SupplyFactoringAgreement> CreateAsync(SupplyFactoringAgreement entity)
        {
            await factoringAgreementValidator.ValidateSupplyAgreementsAsync(entity);
            if (await unitOfWork.Set<FactoringAgreement>()
                .AnyAsync(f => f.Id == entity.FactoringAgreementId && !f.IsActive))
            {
                entity.Status = SupplyFactoringAgreementStatus.NotActive;
            }
            else
            {
                entity.Status = SupplyFactoringAgreementStatus.Active;
            }

            return await unitOfWork.AddAndSaveAsync(entity);
        }

        public async Task<SupplyFactoringAgreement> UpdateAsync(SupplyFactoringAgreement entity)
        {
            await factoringAgreementValidator.ValidateSupplyAgreementsAsync(entity);
            return await unitOfWork.UpdateAndSaveAsync<SupplyFactoringAgreement, Guid>(entity);
        }

        public Task<SupplyFactoringAgreement> Get(Guid id)
        {
            return unitOfWork.GetOrFailAsync<SupplyFactoringAgreement, Guid>(id);
        }

        public Task<SupplyFactoringAgreement[]> GetAll(Guid? factoringAgreementId = null)
        {
            return unitOfWork.Set<SupplyFactoringAgreement>()
                .Where(s => !factoringAgreementId.HasValue || 
                            s.FactoringAgreementId == factoringAgreementId.Value)
                .ToArrayAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            await factoringAgreementValidator.ValidateRemovalAsync(id);
            await unitOfWork.RemoveAndSaveAsync<SupplyFactoringAgreement, Guid>(id);
        }
    }
}