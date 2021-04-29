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

namespace Discounting.Logics.Validators
{
    public interface IContractValidator
    {
        Task ValidateAsync(Contract contract, Guid currentUserId);
        Task ValidateRequestedContractPermission(Guid contractId, Guid currentUserId);
    }

    public class ContractValidator : IContractValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public ContractValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(Contract contract, Guid currentUserId)
        {
            var validationResult = new ValidationResult();
            if (contract.IsRequiredRegistry && !contract.IsFactoring)
            {
                validationResult.Errors.Add(new ValidationError(nameof(Contract.IsRequiredRegistry),
                    "Contract can't have required registry checked if finance type is not Verification",
                    new InvalidErrorDetails())
                );
            }

            if (contract.IsRequiredNotification && !contract.IsFactoring)
            {
                validationResult.Errors.Add(new ValidationError(nameof(Contract.IsRequiredNotification),
                    "Contract can't have required notification checked if finance type is not Verification",
                    new InvalidErrorDetails())
                );
            }

            await foreach (var error in ValidateReferences(contract, currentUserId))
            {
                validationResult.Errors.Add(error);
            }

            //Throw critical errors before preceding to other validations
            validationResult.ThrowIfAny();
            await ValidateExistence(contract.Id, contract.SellerId, contract.BuyerId);
        }

        public async Task ValidateRequestedContractPermission(Guid contractId, Guid currentUserId)
        {
            var currentUser = await unitOfWork.Set<User>()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser.Company.CompanyType == CompanyType.Bank &&
                !await unitOfWork.Set<Registry>()
                    .AnyAsync(c => c.BankId == currentUser.CompanyId &&
                                   c.ContractId == contractId))
            {
                throw new NotFoundException();
            }

            if (!currentUser.IsSuperAdmin &&
                currentUser.Company.CompanyType != CompanyType.Bank &&
                await unitOfWork.Set<Contract>()
                    .AnyAsync(c => c.Id == contractId &&
                                   c.SellerId != currentUser.CompanyId &&
                                   c.BuyerId != currentUser.CompanyId))
            {
                throw new NotFoundException();
            }
        }

        private async IAsyncEnumerable<ValidationError> ValidateReferences(Contract contract, Guid currentUserId)
        {
            if (contract.SellerId == contract.BuyerId)
            {
                yield return new ReferenceNotAllowedError(nameof(contract.SellerId));
            }

            if (contract.Id != default)
            {
                if (!await unitOfWork.Set<Contract>()
                    .AnyAsync(c => c.Id == contract.Id))
                {
                    yield return new NotFoundValidationError(nameof(contract.Id));
                }

                if (await unitOfWork.Set<Contract>()
                    .AnyAsync(c => c.Id == contract.Id &&
                                   c.BuyerId != contract.BuyerId))
                {
                    yield return new DisabledFieldValidationError(nameof(contract.BuyerId));
                }

                if (await unitOfWork.Set<Contract>()
                    .AnyAsync(c => c.Id == contract.Id &&
                                   c.SellerId != contract.SellerId))
                {
                    yield return new DisabledFieldValidationError(nameof(contract.SellerId));
                }
            }
        }

        private async Task ValidateExistence(Guid contractId, Guid sellerId, Guid buyerId)
        {
            if (await unitOfWork.Set<Contract>()
                .AnyAsync(c => c.Id != contractId &&
                               c.SellerId == sellerId &&
                               c.BuyerId == buyerId))
            {
                throw new ValidationException(
                    nameof(Contract.SellerId),
                    "Contract already exists between these companies",
                    new GeneralErrorDetails("contract-already-exists-between-companies"));
            }
        }
    }
}