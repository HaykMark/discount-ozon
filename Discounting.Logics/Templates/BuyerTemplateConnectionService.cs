using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.Templates;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Templates
{
    public interface IBuyerTemplateConnectionService
    {
        Task<(BuyerTemplateConnection[], int)> GetAll(int offset, int limit);
        Task<BuyerTemplateConnection> Get(Guid id);
        Task<BuyerTemplateConnection> CreateAsync(BuyerTemplateConnection entity);
        Task<BuyerTemplateConnection> UpdateAsync(BuyerTemplateConnection entity);
    }

    public class BuyerTemplateConnectionService : IBuyerTemplateConnectionService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBuyerTemplateConnectionValidator buyerTemplateConnectionValidator;
        private readonly ISessionService sessionService;

        public BuyerTemplateConnectionService(
            IUnitOfWork unitOfWork,
            IBuyerTemplateConnectionValidator buyerTemplateConnectionValidator,
            ISessionService sessionService
        )
        {
            this.unitOfWork = unitOfWork;
            this.buyerTemplateConnectionValidator = buyerTemplateConnectionValidator;
            this.sessionService = sessionService;
        }

        private IQueryable<BuyerTemplateConnection> GetBaseQuery()
        {
            return unitOfWork
                .Set<BuyerTemplateConnection>()
                .Include(c => c.Buyer)
                .Include(c => c.Template);
        }

        public async Task<(BuyerTemplateConnection[], int)> GetAll(int offset, int limit)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);

            var query = GetBaseQuery()
                .Where(c => currentUser.IsSuperAdmin ||
                            c.BankId == currentUser.CompanyId ||
                            c.BuyerId == currentUser.CompanyId);
            return (await query
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<BuyerTemplateConnection> Get(Guid id)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await buyerTemplateConnectionValidator
                .ValidateRequestedTemplateConnectionPermissionAsync(id, currentUser.CompanyId);
            return await unitOfWork.GetOrFailAsync(id, GetBaseQuery());
        }

        public async Task<BuyerTemplateConnection> CreateAsync(BuyerTemplateConnection entity)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            entity.BankId = currentUser.CompanyId;
            await buyerTemplateConnectionValidator.ValidateAsync(entity);
            return await unitOfWork.AddAndSaveAsync(entity);
        }

        public async Task<BuyerTemplateConnection> UpdateAsync(BuyerTemplateConnection entity)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            entity.BankId = currentUser.CompanyId;
            await buyerTemplateConnectionValidator.ValidateAsync(entity);
            return await unitOfWork.UpdateAndSaveAsync<BuyerTemplateConnection, Guid>(entity);
        }
    }
}