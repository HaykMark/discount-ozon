using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Templates;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface IRegistryValidator
    {
        void ValidateSupplies(Supply[] supplies, FinanceType financeType, User user, Guid? bankId,
            Guid? factoringAgreementId);

        void ValidateBeforeDeclineAsync(Registry registry, User currentUser);
        Task ValidateBankAsync(Guid bankId, Guid currentUserId);
        Task ValidateTemplateAsync(Registry registry, TemplateType type);
        void ValidateRegistrySupplyChangeAsync(Registry registry, Guid[] supplyIds);
    }

    public class RegistryValidator : IRegistryValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public RegistryValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void ValidateSupplies(Supply[] supplies, FinanceType financeType, User user, Guid? bankId,
            Guid? factoringAgreementId)
        {
            if (!supplies.Any())
            {
                throw new NotFoundException();
            }

            switch (financeType)
            {
                case FinanceType.None:
                    throw new ValidationException(new RequiredFieldValidationError(nameof(FinanceType)));
                case FinanceType.SupplyVerification when !bankId.HasValue || !factoringAgreementId.HasValue:
                    throw new ValidationException(new RequiredFieldValidationError(nameof(bankId)));
                case FinanceType.DynamicDiscounting when bankId.HasValue || factoringAgreementId.HasValue:
                    throw new ValidationException(
                        new DisabledFieldValidationError($"{nameof(bankId)}|{nameof(factoringAgreementId)}"));
            }

            if (supplies.Select(s => s.ContractId).Distinct().Count() != 1)
            {
                throw new ForbiddenException("different-contracts-in-supplies",
                    "Supplies are connected to different contracts");
            }

            if (supplies.First().Contract.SellerId != user.CompanyId)
            {
                throw new ForbiddenException("not-seller",
                    "Cannot add registry because current user is not a seller");
            }

            if (financeType == FinanceType.SupplyVerification)
            {
                var contractNumbers = supplies
                    .Select(s => s.ContractNumber)
                    .Distinct()
                    .ToList();

                if (contractNumbers.Count() != 1)
                {
                    throw new ForbiddenException("different-contract-number-in-supplies",
                        "Supplies are connected to different contract numbers");
                }

                var factoringAgreements = user.Company.FactoringAgreements
                    .Where(f => f.BankId == bankId.Value)
                    .ToList();
                if (!factoringAgreements.Any())
                {
                    throw new NotFoundException(nameof(bankId));
                }

                if (!factoringAgreements.Any(f => f.Id == factoringAgreementId && f.IsActive))
                {
                    throw new NotFoundException(nameof(factoringAgreementId));
                }

                if (factoringAgreements.First(f => f.Id == factoringAgreementId)
                    .SupplyFactoringAgreements.All(f => f.Number != contractNumbers.First()))
                {
                    throw new NotFoundException("SupplyFactoringAgreement");
                }
            }

            foreach (var supply in supplies)
            {
                if (!Supply.IsMainType(supply.Type) &&
                    (!supply.BaseDocumentId.HasValue ||
                     supplies.All(s => s.Id != supply.BaseDocumentId.Value)))
                {
                    throw new ForbiddenException("supply-has-no-base-document", "Some supplies have no base document");
                }

                if (supply.RegistryId.HasValue)
                {
                    throw new ForbiddenException("supply-already-has-registry",
                        "Some supplies already have registries");
                }

                switch (financeType)
                {
                    case FinanceType.SupplyVerification
                        when supply.Contract.IsFactoring &&
                             !supply.Contract.IsRequiredNotification &&
                             !supply.Contract.IsRequiredRegistry:
                        throw new ForbiddenException("supply-contract-is-without-registry",
                            "Some supplies are without registries in contract");
                    case FinanceType.DynamicDiscounting when !supply.Contract.IsDynamicDiscounting:
                        throw new ForbiddenException("supply-contract-is-not-dynamic-discounting",
                            "Some supplies are without dynamic discounting in contract");
                }

                if (supply.DelayEndDate.Date <= DateTime.UtcNow.Date)
                {
                    throw new ForbiddenException("supply-is-not-available",
                        "Some supplies are not available");
                }
            }
        }

        public async Task ValidateBankAsync(Guid bankId, Guid currentUserId)
        {
            var user = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .ThenInclude(c => c.FactoringAgreements)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user.Company.FactoringAgreements.All(f => f.BankId != bankId))
            {
                throw new NotFoundException("BankId");
            }
        }

        public void ValidateBeforeDeclineAsync(Registry registry, User currentUser)
        {
            if (registry.Status == RegistryStatus.Finished)
            {
                throw new ForbiddenException("cannot-decline-registry-is-finished",
                    "Cannot decline because registry is already finished");
            }

            switch (registry.SignStatus)
            {
                case RegistrySignStatus.NotSigned:
                    break;
                case RegistrySignStatus.SignedBySeller:
                    if (registry.Contract.SellerId == currentUser.CompanyId)
                        throw new ForbiddenException("cannot-decline-registry-is-signed",
                            "Cannot decline because registry is already signed by current user");
                    break;
                case RegistrySignStatus.SignedByBuyer:
                    if (registry.Contract.BuyerId == currentUser.CompanyId)
                        throw new ForbiddenException("cannot-decline-registry-is-signed",
                            "Cannot decline because registry is already signed by current user");
                    break;
                case RegistrySignStatus.SignedBySellerBuyer:
                    if (registry.Contract.SellerId == currentUser.CompanyId ||
                        registry.Contract.BuyerId == currentUser.CompanyId)
                        throw new ForbiddenException("cannot-decline-registry-is-signed",
                            "Cannot decline because registry is already signed by current user");
                    break;
            }
        }

        public async Task ValidateTemplateAsync(Registry registry, TemplateType type)
        {
            if (type == TemplateType.Discount && registry.FinanceType != FinanceType.DynamicDiscounting)
            {
                throw new ForbiddenException("discount-template-wrong-finance-type",
                    "Cannot generate discount template because registry has a wrong finance type");
            }

            if (registry.FinanceType == FinanceType.DynamicDiscounting)
            {
                if (!await unitOfWork.Set<Template>()
                    .AnyAsync(t => t.CompanyId == registry.Contract.BuyerId &&
                                   t.Type == type))
                {
                    throw new NotFoundException(typeof(Template));
                }
            }
            else
            {
                if (!await unitOfWork.Set<BuyerTemplateConnection>()
                    .AnyAsync(t => t.BuyerId == registry.Contract.BuyerId &&
                                   t.Template.Type == type))
                {
                    throw new NotFoundException(typeof(Template));
                }
            }
        }

        public void ValidateRegistrySupplyChangeAsync(Registry registry, Guid[] supplyIds)
        {
            if (!supplyIds.Any())
            {
                throw new NotFoundException();
            }

            if (registry.Status != RegistryStatus.InProcess)
            {
                throw new ForbiddenException("registry-status-is-not-in-process",
                    "Cannot set another supplies to this registry. Registry is not in process");
            }
        }
    }
}