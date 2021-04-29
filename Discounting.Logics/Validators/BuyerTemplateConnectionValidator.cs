using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Templates;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface IBuyerTemplateConnectionValidator
    {
        Task ValidateRequestedTemplateConnectionPermissionAsync(Guid id, Guid companyId);
        Task ValidateAsync(BuyerTemplateConnection entity);
    }

    public class BuyerTemplateConnectionValidator : IBuyerTemplateConnectionValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public BuyerTemplateConnectionValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateRequestedTemplateConnectionPermissionAsync(Guid id, Guid companyId)
        {
            if (!await unitOfWork.Set<BuyerTemplateConnection>()
                .AnyAsync(t => t.Id == id && t.Template.CompanyId == companyId))
            {
                throw new NotFoundException(typeof(BuyerTemplateConnection));
            }
        }

        public async Task ValidateAsync(BuyerTemplateConnection entity)
        {
            if (!await unitOfWork.Set<Company>()
                .AnyAsync(c => c.Id == entity.BuyerId &&
                               c.CompanyType == CompanyType.SellerBuyer))
            {
                throw new NotFoundException(typeof(Company));
            }
            
            if (!await unitOfWork.Set<Company>()
                .AnyAsync(c => c.Id == entity.BankId &&
                               c.CompanyType == CompanyType.Bank))
            {
                throw new NotFoundException(typeof(Company));
            }

            if (!await unitOfWork.Set<Company>().AnyAsync(c => c.Id == entity.BankId && c.CompanyType == CompanyType.Bank))
            {
                throw new ForbiddenException("only-bank-can-assign-template",
                    "Only bank type companies can assign templates");
            }

            var template = await unitOfWork.Set<Template>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == entity.TemplateId &&
                    t.CompanyId == entity.BankId &&
                    (t.Type == TemplateType.Verification ||
                     t.Type == TemplateType.Registry));

            if (template is null)
            {
                throw new NotFoundException(typeof(Template));
            }

            if (await unitOfWork.Set<BuyerTemplateConnection>().AnyAsync(t => t.Id != entity.Id &&
                                                                              t.BuyerId == entity.BuyerId &&
                                                                              t.BankId == entity.BankId &&
                                                                              t.Template.Type == template.Type))
            {
                throw new ForbiddenException("buyer-already-has-template",
                    "Buyer already has a template attached.");
            }
        }
    }
}