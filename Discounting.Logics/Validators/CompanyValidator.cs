using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation.Errors;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.Account;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface ICompanyValidator
    {
        Task ValidateAsync(Company company, User user);
        Task ValidateSettingsAsync(CompanySettings settings, User user);
        void ValidateDeactivation(Company company, User currentUser);
        void ValidateActivation(Company company, User currentUser);
    }

    public class CompanyValidator : ICompanyValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(Company company, User user)
        {
            if (!user.IsSuperAdmin)
            {
                if (user.CompanyId != company.Id)
                {
                    throw new NotFoundException();
                }

                if (user.IsConfirmedByAdmin && !company.IsActive)
                {
                    throw new ForbiddenException();
                }

                if (await unitOfWork.Set<Company>().AnyAsync(c => c.Id == company.Id && c.IsActive != company.IsActive))
                {
                    throw new ForbiddenException();
                }
            }
        }

        public async Task ValidateSettingsAsync(CompanySettings settings, User user)
        {
            if (await unitOfWork.Set<CompanySettings>()
                .AnyAsync(c => c.CompanyId == settings.CompanyId &&
                               c.Id != settings.Id))
            {
                throw new ValidationException(nameof(CompanySettings.CompanyId),
                    "Company settings with this company already exists",
                    new DuplicateErrorDetails()
                );
            }

            if (user.CompanyId != settings.CompanyId)
            {
                throw new ForbiddenException();
            }

            // if (settings.IsSendAutomatically &&
            //     !await unitOfWork.Set<FactoringAgreement>().AnyAsync(f => f.CompanyId == user.CompanyId &&
            //                                                         f.Status == BankConnectionStatus.Default))
            // {
            //     throw new ValidationException(nameof(CompanySettings.CompanyId),
            //         "Cant set to Send Automatically because there is no default bank.",
            //         new GeneralErrorDetails("no-default-bank")
            //     );
            // }
        }


        public void ValidateDeactivation(Company company, User currentUser)
        {
            if (!company.IsActive)
            {
                throw new ForbiddenException("company-is-already-deactivated", 
                    "This company is already deactivated");
            }

            if (!currentUser.IsSuperAdmin && currentUser.CompanyId != company.Id)
            {
                throw new ForbiddenException("not-your-company-to-deactivate", 
                    "User can deactivate only their company");
            }

            if (!currentUser.IsAdmin && !currentUser.IsSuperAdmin)
            {
                throw new ForbiddenException("user-is-not-admin", 
                    "Only admin users can deactivate their company");
            }
        }

        public void ValidateActivation(Company company, User currentUser)
        {
            if (company.IsActive)
            {
                throw new ForbiddenException("company-is-already-active", 
                    "This company is already deactivated");
            }

            if (!currentUser.IsSuperAdmin && currentUser.CompanyId != company.Id)
            {
                throw new ForbiddenException("not-your-company-to-activate", 
                    "User can activate only their company");
            }

            if (!currentUser.IsAdmin && !currentUser.IsSuperAdmin)
            {
                throw new ForbiddenException("user-is-not-admin", 
                    "Only admin users can activate their company");
            }
        }
    }
}