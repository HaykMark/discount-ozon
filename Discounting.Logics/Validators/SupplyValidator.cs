using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Common.Validation.Errors;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Discounting.Logics.Validators
{
    public interface ISupplyValidator
    {
        Task<ValidationErrors> ValidateAsync(List<Supply> supplies, Guid currentUserId);
        Task<ValidationErrors> ValidateSellerManualVerificationAsync(List<Supply> supplies, Guid bankId, User user);
        Task<ValidationErrors> ValidateBuyerManualVerificationAsync(List<Supply> supplies, User user);
        Task<ValidationErrors> ValidateAutomaticallyVerificationAsync(List<Supply> supplies, User user);
    }

    public class SupplyValidator : ISupplyValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public SupplyValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ValidationErrors> ValidateAsync(List<Supply> supplies, Guid currentUserId)
        {
            var validationErrors = new ValidationErrors();
            var user = await unitOfWork.Set<User>()
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            validationErrors.AddRange(ValidateArguments(supplies, user.Company.TIN));
            await foreach (var error in ValidateExistenceAsync(supplies, user.Company.TIN))
            {
                validationErrors.Add(error);
            }

            return validationErrors;
        }
        
        public async Task<ValidationErrors> ValidateSellerManualVerificationAsync(List<Supply> supplies, Guid bankId,
            User user)
        {
            var validationErrors = new ValidationErrors();

            if (user.Company.FactoringAgreements.All(f => f.BankId != bankId))
            {
                validationErrors.Add(new NotFoundValidationError("BankId"));
            }

            if (supplies.All(s => s.Contract.Seller.TIN != user.Company.TIN))
            {
                throw new ForbiddenException("different-seller-in-supply",
                    "Verification of supplies is not available, " +
                    "because the current user is not a seller");
            }

            await ValidateSellerVerificationAsync(supplies, validationErrors);
            return validationErrors;
        }

        public async Task<ValidationErrors> ValidateBuyerManualVerificationAsync(List<Supply> supplies, User user)
        {
            var validationErrors = new ValidationErrors();

            if (supplies.All(s => !s.BankId.HasValue))
            {
                throw new NotFoundException(nameof(Supply.BankId));
            }

            if (supplies.All(s => s.Contract.Buyer.TIN != user.Company.TIN))
            {
                throw new ForbiddenException("different-seller-in-supply",
                    "Verification of supplies is not available, " +
                    "because the current user is not a buyer");
            }

            await ValidateBuyerVerificationAsync(supplies, validationErrors);
            return validationErrors;
        }

        public async Task<ValidationErrors> ValidateAutomaticallyVerificationAsync(List<Supply> supplies, User user)
        {
            var validationErrors = new ValidationErrors();
            //
            // if (user.Company.FactoringAgreements.All(f => f.Status != BankConnectionStatus.Default))
            // {
            //     throw new NotFoundException("BankId");
            // }

            if (!user.Company.CompanySettings.IsSendAutomatically)
            {
                throw new ForbiddenException("not-set-to-automatic-verification",
                    "Cannot verify automatically because the setting is off");
            }

            await ValidateSellerVerificationAsync(supplies, validationErrors);

            return validationErrors;
        }

        private async Task ValidateSellerVerificationAsync(List<Supply> supplies, ValidationErrors errors)
        {
            var unprocessableSupplies = new HashSet<Guid>();
            foreach (var supply in supplies)
            {
                if (supply.Contract.IsRequiredRegistry ||
                    supply.Contract.IsRequiredNotification)
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new ValidationError(supply.Contract.IsRequiredRegistry 
                            ? nameof(Contract.IsRequiredRegistry) 
                            : nameof(Contract.IsRequiredNotification),
                        "Cannot verify supplies for registries",
                        new GeneralErrorDetails("supplies-are-for-registries", new { supply.Id })));
                    continue;
                }

                if (!await unitOfWork.Set<Supply>().AnyAsync(s => s.Id == supply.Id &&
                                                                  s.Status == SupplyStatus.InProcess))
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new NotFoundValidationError(nameof(Supply.Id)));
                }
                
                if (supply.SellerVerified)
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new ValidationError(nameof(Supply.Number),
                        new GeneralErrorDetails("already-verified-by-seller", new {Id = supply.Id})));
                }

                if (!supply.Contract.IsFactoring)
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new ValidationError(nameof(Contract.IsFactoring),
                        "Wrong finance type for the current supply",
                        new GeneralErrorDetails("wrong-finance-type", new {Id = supply.Id})));
                }
            }
            supplies.RemoveAll(s => unprocessableSupplies.Contains(s.Id));

        }
        
        private async Task ValidateBuyerVerificationAsync(List<Supply> supplies, ValidationErrors errors)
        {
            var unprocessableSupplies = new HashSet<Guid>();
            foreach (var supply in supplies)
            {
                if (supply.Contract.IsRequiredRegistry ||
                    supply.Contract.IsRequiredNotification)
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new ValidationError(supply.Contract.IsRequiredRegistry 
                            ? nameof(Contract.IsRequiredRegistry) 
                            : nameof(Contract.IsRequiredNotification),
                        "Cannot verify supplies for registries",
                        new GeneralErrorDetails("supplies-are-for-registries", new { supply.Id })));
                    continue;
                }
                if (!await unitOfWork.Set<Supply>().AnyAsync(s => s.Id == supply.Id &&
                                                                  s.Status == SupplyStatus.InProcess))
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new NotFoundValidationError(nameof(Supply.Id)));
                }

                if (supply.AddedBySeller && !supply.SellerVerified)
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new ValidationError(nameof(Supply.Number),
                        new GeneralErrorDetails("not-verified-by-seller", new {Id = supply.Id})));
                }
                
                if (supply.BuyerVerified)
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new ValidationError(nameof(Supply.Number),
                        new GeneralErrorDetails("already-verified-by-buyer", new {Id = supply.Id})));
                }

                if (!await unitOfWork.Set<Company>()
                    .AnyAsync(c => c.Id == supply.BankId.Value && c.CompanyType == CompanyType.Bank))
                {
                    unprocessableSupplies.Add(supply.Id);
                    errors.Add(new ValidationError(nameof(Supply.Number),
                        new NotFoundErrorDetails(nameof(Supply.BankId))));
                }
            }
            supplies.RemoveAll(s => unprocessableSupplies.Contains(s.Id));
        }

        private async IAsyncEnumerable<ValidationError> ValidateExistenceAsync(List<Supply> supplies,
            string currentUserTin)
        {
            var unprocessableSupplies = new HashSet<Guid>();
            foreach (var supply in supplies)
            {
                var contract = await unitOfWork.Set<Contract>()
                    .FirstOrDefaultAsync(c =>
                        c.Seller.TIN == supply.Contract.Seller.TIN &&
                        c.Buyer.TIN == supply.Contract.Buyer.TIN);

                if (supply.Contract.Seller.TIN == currentUserTin &&
                    contract == null)
                {
                    unprocessableSupplies.Add(supply.Id);
                    yield return new ValidationError(nameof(Supply.Number), new GeneralErrorDetails(
                        "contract-not-found",
                        new SupplyErrorArgs
                        {
                            DOCUMENT_NUMBER = supply.Number,
                            DOCUMENT_TYPE = supply.Type,
                            DOCUMENT_DATE = supply.Date
                        }));
                }

                if (contract != null &&
                    await unitOfWork.Set<Supply>()
                        .AnyAsync(s => s.ContractId == contract.Id &&
                                       s.Number == supply.Number &&
                                       s.Type == supply.Type &&
                                       s.Date.Date == supply.Date.Date))
                {
                    unprocessableSupplies.Add(supply.Id);
                    yield return new ValidationError(nameof(Supply.Number), new GeneralErrorDetails(
                        "supply-already-exists",
                        new SupplyErrorArgs
                        {
                            DOCUMENT_NUMBER = supply.Number,
                            DOCUMENT_TYPE = supply.Type,
                            DOCUMENT_DATE = supply.Date
                        }));
                }
            }

            supplies.RemoveAll(s => unprocessableSupplies.Contains(s.Id));
        }

        private IEnumerable<ValidationError> ValidateArguments(List<Supply> supplies, string currentUserTin)
        {
            var unprocessableSupplies = new HashSet<Guid>();
            foreach (var supply in supplies)
            {
                if (supply.Contract.Seller.TIN != currentUserTin &&
                    supply.Contract.Buyer.TIN != currentUserTin)
                {
                    if (!unprocessableSupplies.Contains(supply.Id))
                    {
                        unprocessableSupplies.Add(supply.Id);
                    }

                    yield return new ValidationError("SellerTin", new GeneralErrorDetails(
                        "supply-does-not-belong-to-current-company",
                        new SupplyErrorArgs
                        {
                            DOCUMENT_NUMBER = supply.Number,
                            DOCUMENT_TYPE = supply.Type,
                            DOCUMENT_DATE = supply.Date
                        }));
                }

                if (supply.Contract.Seller.TIN == supply.Contract.Buyer.TIN)
                {
                    if (!unprocessableSupplies.Contains(supply.Id))
                    {
                        unprocessableSupplies.Add(supply.Id);
                    }

                    yield return new ValidationError("SellerTin", new GeneralErrorDetails(
                        "same-seller-and-buyer-tin",
                        new SupplyErrorArgs
                        {
                            DOCUMENT_NUMBER = supply.Number,
                            DOCUMENT_TYPE = supply.Type,
                            DOCUMENT_DATE = supply.Date
                        }));
                }

                if (supply.Type != SupplyType.Ukd && supply.Amount < 0)
                {
                    yield return new ValidationError(nameof(Supply.Amount), new GeneralErrorDetails(
                        "amount-is-negative",
                        new SupplyErrorArgs
                        {
                            DOCUMENT_NUMBER = supply.Number,
                            DOCUMENT_TYPE = supply.Type,
                            DOCUMENT_DATE = supply.Date
                        }));
                }

                if (supply.Amount == 0)
                {
                    if (!unprocessableSupplies.Contains(supply.Id))
                    {
                        unprocessableSupplies.Add(supply.Id);
                    }

                    yield return new ValidationError(nameof(Supply.Amount), new GeneralErrorDetails(
                        "amount-is-zero",
                        new SupplyErrorArgs
                        {
                            DOCUMENT_NUMBER = supply.Number,
                            DOCUMENT_TYPE = supply.Type,
                            DOCUMENT_DATE = supply.Date
                        }));
                }

                if (!Supply.IsMainType(supply.Type))
                {
                    if (string.IsNullOrEmpty(supply.BaseDocumentNumber))
                    {
                        if (!unprocessableSupplies.Contains(supply.Id))
                        {
                            unprocessableSupplies.Add(supply.Id);
                        }

                        yield return new RequiredFieldValidationError(nameof(Supply.BaseDocumentNumber),
                            new SupplyErrorArgs
                            {
                                DOCUMENT_NUMBER = supply.Number,
                                DOCUMENT_TYPE = supply.Type,
                                DOCUMENT_DATE = supply.Date
                            });
                    }

                    if (!supply.BaseDocumentDate.HasValue)
                    {
                        if (!unprocessableSupplies.Contains(supply.Id))
                        {
                            unprocessableSupplies.Add(supply.Id);
                        }

                        yield return new RequiredFieldValidationError(nameof(Supply.BaseDocumentDate),
                            new SupplyErrorArgs
                            {
                                DOCUMENT_NUMBER = supply.Number,
                                DOCUMENT_TYPE = supply.Type,
                                DOCUMENT_DATE = supply.Date
                            });
                    }

                    if (supply.Type == SupplyType.None || !Supply.IsMainType(supply.BaseDocumentType))
                    {
                        if (!unprocessableSupplies.Contains(supply.Id))
                        {
                            unprocessableSupplies.Add(supply.Id);
                        }

                        yield return new RequiredFieldValidationError(nameof(Supply.BaseDocumentType),
                            new SupplyErrorArgs
                            {
                                DOCUMENT_NUMBER = supply.Number,
                                DOCUMENT_TYPE = supply.Type,
                                DOCUMENT_DATE = supply.Date
                            });
                    }

                    else if ((supply.Type == SupplyType.Invoice &&
                              supply.BaseDocumentType != SupplyType.Akt &&
                              supply.BaseDocumentType != SupplyType.Torg12) ||
                             (supply.Type == SupplyType.Ukd && supply.BaseDocumentType != SupplyType.Upd))
                    {
                        if (!unprocessableSupplies.Contains(supply.Id))
                        {
                            unprocessableSupplies.Add(supply.Id);
                        }

                        yield return new ValidationError(nameof(Supply.BaseDocumentType),
                            new InvalidErrorDetails(
                                new SupplyErrorArgs
                                {
                                    DOCUMENT_NUMBER = supply.Number,
                                    DOCUMENT_TYPE = supply.Type,
                                    DOCUMENT_DATE = supply.Date
                                }));
                    }
                }
            }

            supplies.RemoveAll(s => unprocessableSupplies.Contains(s.Id));
        }
    }
}