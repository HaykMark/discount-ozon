using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.Account;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.CompanyServices
{
    public interface ICompanyAggregateService
    {
        Task<TEntity[]> Get<TEntity>(Guid companyId)
            where TEntity : class, ICompanyAggregate;

        Task<TEntity> CreateAsync<TEntity>(TEntity companyAggregate)
            where TEntity : class, ICompanyAggregate;

        Task<TEntity> UpdateAsync<TEntity>(TEntity companyAggregate)
            where TEntity : class, ICompanyAggregate;
    }

    public class CompanyAggregateService : ICompanyAggregateService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;

        public CompanyAggregateService(IUnitOfWork unitOfWork, ISessionService sessionService)
        {
            this.unitOfWork = unitOfWork;
            this.sessionService = sessionService;
        }

        public async Task<TEntity[]> Get<TEntity>(Guid companyId)
            where TEntity : class, ICompanyAggregate
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.Set<User>()
                .Where(u => u.Id == currentUserId)
                .FirstAsync();
            if (!user.IsSuperAdmin && companyId != user.CompanyId)
            {
                throw new ForbiddenException();
            }

            var aggregates = await unitOfWork.Set<TEntity>().Where(e => e.CompanyId == companyId).ToArrayAsync();
            if (!aggregates.Any())
            {
                throw new NotFoundException();
            }

            return aggregates;
        }

        public async Task<TEntity> CreateAsync<TEntity>(TEntity companyAggregate)
            where TEntity : class, ICompanyAggregate
        {
            await ValidateExistenceAsync(companyAggregate);
            return await unitOfWork.AddAndSaveAsync(companyAggregate);
        }

        public async Task<TEntity> UpdateAsync<TEntity>(TEntity companyAggregate)
            where TEntity : class, ICompanyAggregate
        {
            return await unitOfWork.UpdateAndSaveAsync<TEntity, Guid>(companyAggregate);
        }

        private async Task ValidateExistenceAsync<TEntity>(TEntity companyAggregate)
            where TEntity : class, ICompanyAggregate
        {
            if (!(companyAggregate is MigrationCardInfo) &&
                !(companyAggregate is ResidentPassportInfo))
            {
                if (await unitOfWork.Set<TEntity>().AnyAsync(e => e.CompanyId == companyAggregate.CompanyId))
                {
                    throw new ForbiddenException();
                }
            }
            else if (await unitOfWork.Set<TEntity>().CountAsync(e => e.CompanyId == companyAggregate.CompanyId) == 2)
            {
                throw new ForbiddenException();
            }
        }
    }
}