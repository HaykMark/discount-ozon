using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Templates;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface ITemplateValidator
    {
        Task ValidateAsync(Guid id, Guid companyId, TemplateType type);
        Task ValidateRemoval(Template entity);
    }

    public class TemplateValidator : ITemplateValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public TemplateValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(Guid id, Guid companyId, TemplateType type)
        {
            if (type == TemplateType.Registry || type == TemplateType.Verification)
            {
                if (await unitOfWork.Set<Company>().AnyAsync(c => c.Id == companyId &&
                                                                  c.CompanyType != CompanyType.Bank))
                {
                    throw new ForbiddenException("company-is-not-a-bank",
                        "Template can only be attached to Banks");
                }
            }


            else if (await unitOfWork.Set<Template>().AnyAsync(t => t.Id != id &&
                                                                    t.Type == type &&
                                                                    t.CompanyId == companyId))
            {
                throw new ForbiddenException("duplicate-template",
                    "Template with the same type and company already exists");
            }
        }

        public async Task ValidateRemoval(Template entity)
        {
            switch (entity.Type)
            {
                case TemplateType.Registry:
                    if (await unitOfWork.Set<BuyerTemplateConnection>().AnyAsync(b => b.TemplateId == entity.Id))
                    {
                        throw new ForbiddenException("cannot-remove-template-is-connected-to-buyer",
                            "You are not allowed to remove template, because it is connected to buyer.");
                    }
                    break;
                case TemplateType.ProfileRegulationSellerBuyer:
                case TemplateType.ProfileRegulationPrivateCompany:
                case TemplateType.ProfileRegulationBank:
                    throw new ForbiddenException("cannot-remove-regulation-template",
                        "You are not allowed to remove regulation templates.");
            }
        }
    }
}