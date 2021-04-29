using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.Auditing;
using Discounting.Entities.TariffDiscounting;
using Discounting.Entities.Templates;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.TariffDiscounting
{
    public interface IDiscountService
    {
        string IPAddress { get; set; }

        Task<Discount> Get(Guid id);
        Task<(Discount[], int)> GetAll(int offset, int limit);
        Task<Discount> CreateAsync(Discount entity);
        Task<Discount> UpdateAsync(Discount entity);
        Task RemoveAsync(Guid id);
        Task<SupplyDiscount[]> CreateSupplyDiscountAsync(Guid discountId, SupplyDiscount[] supplyDiscounts);
    }

    public class DiscountService : IDiscountService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IDiscountValidator discountValidator;
        private readonly ISessionService sessionService;
        private readonly ISignatureService signatureService;
        private readonly IAuditService auditService;

        public string IPAddress { get; set; }

        public DiscountService(
            IUnitOfWork unitOfWork,
            IDiscountValidator discountValidator,
            ISessionService sessionService,
            ISignatureService signatureService,
            IAuditService auditService
        )
        {
            this.unitOfWork = unitOfWork;
            this.discountValidator = discountValidator;
            this.sessionService = sessionService;
            this.signatureService = signatureService;
            this.auditService = auditService;
        }

        public async Task<(Discount[], int)> GetAll(int offset, int limit)
        {
            return (await unitOfWork.Set<Discount>()
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await unitOfWork.Set<Discount>().CountAsync());
        }

        public Task<Discount> Get(Guid id)
        {
            return unitOfWork.GetOrFailAsync<Discount, Guid>(id);
        }

        public async Task<Discount> CreateAsync(Discount entity)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            entity.Registry = await unitOfWork.GetOrFailAsync(entity.RegistryId,
                unitOfWork.Set<Registry>()
                    .Include(r => r.Contract)
                    .Include(r => r.Supplies));
            await discountValidator.ValidateAsync(entity, currentUserId);
            //await InitSupplies(entity);
            entity.Registry = null;
            return await unitOfWork.AddAndSaveAsync(entity);
        }

        public async Task<Discount> UpdateAsync(Discount entity)
        {
            var confirmed = false;
            var confirmedWithPercentageChanged = false;
            var currentUserId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            entity.Registry = await unitOfWork.GetOrFailAsync(entity.RegistryId,
                unitOfWork.Set<Registry>()
                    .Include(r => r.Contract)
                    .Include(r => r.Supplies)
                    .ThenInclude(s => s.SupplyDiscount));
            await discountValidator.ValidateAsync(entity, currentUserId);
            if (!entity.Registry.IsConfirmed && user.CompanyId == entity.Registry.Contract.BuyerId)
            {
                confirmed = true;
            }

            entity.Registry.IsConfirmed = confirmed;
            if (await unitOfWork.Set<Discount>().AnyAsync(e => e.Id == entity.Id &&
                                                               e.Rate != entity.Rate))
            {
                confirmedWithPercentageChanged = true;
                await signatureService.RemoveIfAnyAsync(SignatureType.Registry, entity.RegistryId);
                entity.Registry.SignStatus = RegistrySignStatus.NotSigned;
            }

            var discount = await unitOfWork.UpdateAndSaveAsync<Discount, Guid>(entity);
            await auditService.CreateAsync(new Audit
            {
                UserId = currentUserId,
                Incident = confirmedWithPercentageChanged
                    ? IncidentType.DiscountConfirmedPercentageChanged
                    : confirmed
                        ? IncidentType.DiscountRegistryConfirmed
                        : IncidentType.DiscountRegistryUpdated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = IPAddress,
                SourceId = discount.Id.ToString(),
                Message = await auditService.GetMessageAsync<Discount>(discount.Id)
            });
            return discount;
        }

        public async Task<SupplyDiscount[]> CreateSupplyDiscountAsync(Guid discountId, SupplyDiscount[] supplyDiscounts)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await discountValidator.ValidateSupplyDiscountAsync(discountId, currentUser.CompanyId, supplyDiscounts);
            var discountsToAdd = supplyDiscounts.Where(s => s.Id == default).ToList();
            var discountsToUpdate = supplyDiscounts.Where(s => s.Id != default).ToList();
            await unitOfWork.Set<SupplyDiscount>().AddRangeAsync(discountsToAdd);
            unitOfWork.Set<SupplyDiscount>().UpdateRange(discountsToUpdate);
            await unitOfWork.SaveChangesAsync();
            if(!discountsToUpdate.Any())
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = currentUserId,
                    Incident = IncidentType.DiscountCreated,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = IPAddress,
                    SourceId = discountId.ToString(),
                    Message = await auditService.GetMessageAsync<Discount>(discountId)
                });
            }
            return discountsToAdd.Concat(discountsToUpdate).ToArray();
        }

        public async Task RemoveAsync(Guid id)
        {
            await unitOfWork.RemoveAndSaveAsync<Discount, Guid>(id);
        }
    }
}