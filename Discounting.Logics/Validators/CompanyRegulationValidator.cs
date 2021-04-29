using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.CompanyAggregates;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface ICompanyRegulationValidator
    {
        Task ValidateForProfileTemplate(Company company);
    }

    public class CompanyRegulationValidator : ICompanyRegulationValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyRegulationValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateForProfileTemplate(Company company)
        {
            if (company is null)
            {
                throw new NotFoundException(typeof(Company));
            }

            if (company.HasPowerOfAttorney && company.CompanyAuthorizedUserInfo is null)
            {
                throw new NotFoundException(typeof(CompanyAuthorizedUserInfo));
            }

            if (company.CompanyContactInfo is null)
            {
                throw new NotFoundException(typeof(CompanyContactInfo));
            }

            if (company.CompanyOwnerPositionInfo is null)
            {
                throw new NotFoundException(typeof(CompanyOwnerPositionInfo));
            }

            if (((company.HasPowerOfAttorney && company.CompanyAuthorizedUserInfo.IsResident) ||
                 company.CompanyOwnerPositionInfo.IsResident) &&
                (company.ResidentPassportInfos is null || !company.ResidentPassportInfos.Any()))
            {
                throw new NotFoundException(typeof(ResidentPassportInfo));
            }

            if (((company.HasPowerOfAttorney && !company.CompanyAuthorizedUserInfo.IsResident) ||
                 !company.CompanyOwnerPositionInfo.IsResident) &&
                (company.MigrationCardInfos is null || !company.MigrationCardInfos.Any()))
            {
                throw new NotFoundException(typeof(MigrationCardInfo));
            }

            var regulationTemplate = await unitOfWork.Set<CompanyRegulation>()
                .FirstOrDefaultAsync(c => c.Type == CompanyRegulationType.Profile &&
                                          c.CompanyId == company.Id);

            if (regulationTemplate != null &&
                await unitOfWork.Set<CompanyRegulationSignature>()
                    .AnyAsync(s => s.Type == SignatureType.CompanyRegulation &&
                                   s.CompanyRegulationId == regulationTemplate.Id))
            {
                throw new ForbiddenException("profile-regulation-is-already-signed",
                    "You are not allowed to generate new Profile regulation because it is already signed");
            }
        }
    }
}