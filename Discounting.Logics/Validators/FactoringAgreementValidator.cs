using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface IFactoringAgreementValidator
    {
        Task ValidateAsync(FactoringAgreement factoringAgreement, User user);
        Task ValidateSupplyAgreementsAsync(SupplyFactoringAgreement supplyFactoringAgreement);
        void ValidateBankConfirmation(FactoringAgreement agreement, User bank);
        Task ValidateRemovalAsync(Guid supplyFactoringId);
    }

    public class FactoringAgreementValidator : IFactoringAgreementValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public FactoringAgreementValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(FactoringAgreement factoringAgreement, User user)
        {
            if (!user.IsSuperAdmin &&
                (factoringAgreement.CompanyId != user.CompanyId &&
                 factoringAgreement.BankId != user.CompanyId))
            {
                throw new ForbiddenException();
            }

            if (await unitOfWork.Set<FactoringAgreement>()
                .AnyAsync(f => f.Id != factoringAgreement.Id &&
                               f.BankId == factoringAgreement.BankId &&
                               f.FactoringContractNumber ==
                               factoringAgreement.FactoringContractNumber))
            {
                throw new ForbiddenException("cannot-create-agreement-number-exists",
                    "Cannot create, agreement with the same number and bank already exists.");
            }

            if (factoringAgreement.SupplyFactoringAgreements.Any())
            {
                if (factoringAgreement.SupplyFactoringAgreements.Count() !=
                    factoringAgreement.SupplyFactoringAgreements.Select(s => s.Number).Distinct().Count())
                {
                    throw new ForbiddenException("cannot-create-supply-agreement-number-exists",
                        "Cannot create, supply agreement with the same number and factoring agreement already exists.");
                }

                foreach (var supplyFactoringAgreement in factoringAgreement.SupplyFactoringAgreements)
                {
                    if (await unitOfWork.Set<SupplyFactoringAgreement>()
                        .AnyAsync(f => f.Id != supplyFactoringAgreement.Id &&
                                       f.FactoringAgreementId == supplyFactoringAgreement.FactoringAgreementId &&
                                       f.Number == supplyFactoringAgreement.Number))
                    {
                        throw new ForbiddenException("cannot-create-supply-agreement-number-exists",
                            "Cannot create, supply agreement with the same number and factoring agreement already exists.");
                    }
                }
            }

            if (factoringAgreement.Id != default)
            {
                if (factoringAgreement.IsConfirmed &&
                    user.CompanyId != factoringAgreement.BankId &&
                    await unitOfWork.Set<FactoringAgreement>().AnyAsync(f => f.Id == factoringAgreement.Id &&
                                                                             !f.IsConfirmed))
                {
                    throw new ForbiddenException("cannot-confirm-is-not-bank",
                        "Cannot confirm current, user is not the right bank.");
                }

                if (user.CompanyId != factoringAgreement.BankId &&
                    await unitOfWork.Set<FactoringAgreement>().AnyAsync(f => f.Id == factoringAgreement.Id &&
                                                                             f.IsActive != factoringAgreement.IsActive))
                {
                    throw new ForbiddenException("cannot-activate-or-deactivate-is-not-bank",
                        "Cannot activate/deactivate, current user is not the right bank.");
                }
            }

            if (factoringAgreement != null &&
                !await unitOfWork.Set<Company>()
                    .AnyAsync(c => c.CompanyType == CompanyType.Bank &&
                                   c.Id == factoringAgreement.BankId))
            {
                throw new NotFoundException(nameof(FactoringAgreement.Bank));
            }
        }

        public async Task ValidateSupplyAgreementsAsync(SupplyFactoringAgreement supplyFactoringAgreement)
        {
            if (!await unitOfWork.Set<FactoringAgreement>()
                .AnyAsync(f => f.Id == supplyFactoringAgreement.FactoringAgreementId))
            {
                throw new NotFoundException(typeof(FactoringAgreement));
            }

            if (await unitOfWork.Set<SupplyFactoringAgreement>()
                .AnyAsync(f => f.Id != supplyFactoringAgreement.Id &&
                               f.FactoringAgreementId == supplyFactoringAgreement.FactoringAgreementId &&
                               f.Number == supplyFactoringAgreement.Number))
            {
                throw new ForbiddenException("cannot-create-supply-agreement-number-exists",
                    "Cannot create, supply agreement with the same number and factoring agreement already exists.");
            }
        }

        public void ValidateBankConfirmation(FactoringAgreement agreement, User bank)
        {
            if (bank.Company.CompanyType != CompanyType.Bank)
            {
                throw new ForbiddenException("not-a-bank", "Only bank can confirm factoring agreement");
            }

            if (agreement.BankId != bank.CompanyId)
            {
                throw new ForbiddenException("not-a-current-bank", "Only current agreements bank can confirm");
            }

            if (agreement.IsActive)
            {
                throw new ForbiddenException("agreement-is-active", "Only not active agreements can be confirmed");
            }
        }

        public async Task ValidateRemovalAsync(Guid supplyFactoringId)
        {
            var factoringAgreement = await unitOfWork.Set<FactoringAgreement>()
                .Where(f => f.SupplyFactoringAgreements
                    .Any(s => s.Id == supplyFactoringId))
                .FirstOrDefaultAsync();
            if (factoringAgreement != null &&
                await unitOfWork.Set<Registry>()
                    .AnyAsync(r => r.FactoringAgreementId == factoringAgreement.Id &&
                                   r.Status != RegistryStatus.Declined))
            {
                throw new ForbiddenException("factoring-agreement-is-used-in-registry",
                    "You are not allowed to removed Supply agreements that are used in Registries");
            }
        }
    }
}