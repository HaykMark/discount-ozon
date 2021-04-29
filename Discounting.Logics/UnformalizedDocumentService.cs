using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics
{
    public interface IUnformalizedDocumentService
    {
        Task<(UnformalizedDocument[], int)> GetListAsync(
            int offset,
            int limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? creationDateFrom = null,
            DateTime? creationDateTo = null,
            Guid? receiverId = null
        );

        Task<(UnformalizedDocument[], int)> GetSentAsync(
            int offset,
            int limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? sentDateFrom = null,
            DateTime? sentDateTo = null,
            Guid? receiverId = null
        );

        Task<(UnformalizedDocument[], int)> GetReceivedAsync(
            int offset,
            int limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? receivedDateFrom = null,
            DateTime? receivedDateTo = null,
            Guid? senderId = null
        );

        Task<UnformalizedDocument> GetAsync(Guid id);
        Task<UnformalizedDocument> GetDetailedAsync(Guid id);
        Task<UnformalizedDocument> CreateAsync(UnformalizedDocument entity);
        Task<UnformalizedDocument> UpdateAsync(UnformalizedDocument entity);
        Task<UnformalizedDocument> DeclineAsync(UnformalizedDocument entity);
        Task RemoveAsync(Guid id);
    }

    public class UnformalizedDocumentService : IUnformalizedDocumentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;
        private readonly IUnformalizedDocumentValidator validator;
        private readonly IUploadService uploadService;
        private readonly DbSet<UnformalizedDocument> dbSet;

        public UnformalizedDocumentService(
            ISessionService sessionService,
            IUnitOfWork unitOfWork,
            IUnformalizedDocumentValidator validator,
            IUploadService uploadService
        )
        {
            this.sessionService = sessionService;
            this.unitOfWork = unitOfWork;
            this.validator = validator;
            this.uploadService = uploadService;
            dbSet = unitOfWork.Set<UnformalizedDocument>();
        }

        private IQueryable<UnformalizedDocument> GetBaseQuery() =>
            dbSet.Include(d => d.Receivers);

        public async Task<(UnformalizedDocument[], int)> GetListAsync(
            int offset,
            int limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? creationDateFrom = null,
            DateTime? creationDateTo = null,
            Guid? receiverId = null
        )
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            var query = GetBaseQuery()
                .Where(d =>
                    (status == UnformalizedDocumentStatus.None || d.Status == status) &&
                    (!creationDateFrom.HasValue || d.CreationDate >= creationDateFrom.Value) &&
                    (!creationDateTo.HasValue || d.CreationDate <= creationDateTo.Value) &&
                    (!receiverId.HasValue || d.Receivers.Any(r => r.ReceiverId == receiverId.Value))
                );
            if (!currentUser.IsSuperAdmin)
            {
                query = query
                    .Where(d =>
                        d.SenderId == currentUser.CompanyId ||
                        (d.IsSent &&
                         d.Receivers.Any(r => r.ReceiverId == currentUser.CompanyId)));
            }

            return (await query
                    .OrderByDescending(u => u.CreationDate)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<(UnformalizedDocument[], int)> GetSentAsync(
            int offset,
            int limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? sentDateFrom = null,
            DateTime? sentDateTo = null,
            Guid? receiverId = null
        )
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            var query = GetBaseQuery()
                .Where(d => d.SenderId == currentUser.CompanyId &&
                            (status == UnformalizedDocumentStatus.None || d.Status == status) &&
                            d.Status != UnformalizedDocumentStatus.Draft &&
                            (!sentDateFrom.HasValue || d.SentDate >= sentDateFrom.Value) &&
                            (!sentDateTo.HasValue || d.SentDate <= sentDateTo.Value) &&
                            (!receiverId.HasValue || d.Receivers.Any(r => r.ReceiverId == receiverId.Value))
                );

            return (await query
                    .OrderByDescending(u => u.SentDate)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<(UnformalizedDocument[], int)> GetReceivedAsync(
            int offset,
            int limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? receivedDateFrom = null,
            DateTime? receivedDateTo = null,
            Guid? senderId = null
        )
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            var query = GetBaseQuery()
                .Where(d => d.Receivers.Any(r => r.ReceiverId == currentUser.CompanyId) &&
                            (status == UnformalizedDocumentStatus.None || d.Status == status) &&
                            d.Status != UnformalizedDocumentStatus.Draft &&
                            (!receivedDateFrom.HasValue || d.SentDate >= receivedDateFrom.Value) &&
                            (!receivedDateTo.HasValue || d.SentDate <= receivedDateTo.Value) &&
                            (!senderId.HasValue || d.SenderId == senderId.Value)
                );
            return (await query
                    .OrderByDescending(u => u.SentDate)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<UnformalizedDocument> GetAsync(Guid id)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await validator.ValidateRequestedDocumentPermission(id, currentUser.CompanyId);
            return await unitOfWork.GetOrFailAsync(id, GetBaseQuery());
        }

        public async Task<UnformalizedDocument> GetDetailedAsync(Guid id)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await validator.ValidateRequestedDocumentPermission(id, currentUser.CompanyId);
            return await unitOfWork.GetOrFailAsync(id,
                dbSet
                    .Include(d => d.Sender)
                    .ThenInclude(d => d.Users)
                    .Include(d => d.Receivers)
                    .ThenInclude(r => r.Receiver)
                    .ThenInclude(r => r.Users));
        }

        public async Task<UnformalizedDocument> CreateAsync(UnformalizedDocument entity)
        {
            await InitCreateAsync(entity);
            await validator.ValidateAsync(entity);
            var unformalizedDocument = await unitOfWork.AddAndSaveAsync(entity);
            return await unitOfWork.GetOrFailAsync(unformalizedDocument.Id, GetBaseQuery());
        }

        public async Task<UnformalizedDocument> UpdateAsync(UnformalizedDocument entity)
        {
            await validator.ValidateAsync(entity);
            await InitUpdateAsync(entity);
            await unitOfWork.UpdateAndSaveAsync<UnformalizedDocument, Guid>(entity,
                u => u.CreationDate,
                u => u.DeclinedBy,
                u => u.DeclinedDate,
                u => u.DeclineReason
            );
            return await unitOfWork.GetOrFailAsync(entity.Id, GetBaseQuery());
        }

        public async Task<UnformalizedDocument> DeclineAsync(UnformalizedDocument entity)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await validator.ValidateBeforeDeclining(entity, currentUser.CompanyId);
            entity.DeclinedBy = currentUser.CompanyId;
            entity.DeclinedDate = DateTime.UtcNow;
            entity.Status = UnformalizedDocumentStatus.Declined;
            await unitOfWork.UpdateAndSaveAsync<UnformalizedDocument, Guid>(entity, u => u.CreationDate);
            return await unitOfWork.GetOrFailAsync(entity.Id, GetBaseQuery());
        }

        public async Task RemoveAsync(Guid id)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await validator.ValidateBeforeDeleteAsync(id, currentUser.CompanyId);
            await unitOfWork.RemoveAndSaveAsync<UnformalizedDocument, Guid>(id);

            //Remove uploads and files
            var uploads = await unitOfWork.Set<Upload>()
                .Where(u =>
                    u.Provider == UploadProvider.UnformalizedDocument &&
                    u.ProviderId == id)
                .ToListAsync();
            if (uploads.Any())
            {
                await uploadService.RemoveAsync(uploads);
            }
        }

        private async Task InitCreateAsync(UnformalizedDocument entity)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            entity.CreationDate = DateTime.UtcNow;
            entity.Status = UnformalizedDocumentStatus.Draft;
            entity.SenderId = currentUser.CompanyId;
            foreach (var receiver in entity.Receivers)
            {
                receiver.IsSigned = false;
            }
        }

        private async Task InitUpdateAsync(UnformalizedDocument entity)
        {
            entity.Status = UnformalizedDocumentStatus.Draft;
            foreach (var receiver in entity.Receivers)
            {
                receiver.IsSigned = false;
            }
            //
            // var entry = await unitOfWork.GetOrFailAsync<UnformalizedDocument, Guid>(entity.Id);
            // entity.CreationDate = entry.

            await unitOfWork.UpdateNestedCollectionAsync(entity.Receivers, p => p.UnformalizedDocumentId == entity.Id);
        }
    }
}