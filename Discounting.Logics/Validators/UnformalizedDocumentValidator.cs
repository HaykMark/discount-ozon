using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.CompanyAggregates;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface IUnformalizedDocumentValidator
    {
        Task ValidateAsync(UnformalizedDocument entity);
        Task ValidateRequestedDocumentPermission(Guid id, Guid companyId);
        Task ValidateBeforeDeclining(UnformalizedDocument entity, Guid companyId);
        Task ValidateBeforeDeleteAsync(Guid id, Guid companyId);
    }

    public class UnformalizedDocumentValidator : IUnformalizedDocumentValidator
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IReferenceValidator referenceValidator;

        public UnformalizedDocumentValidator(IUnitOfWork unitOfWork, IReferenceValidator referenceValidator)
        {
            this.unitOfWork = unitOfWork;
            this.referenceValidator = referenceValidator;
        }

        public async Task ValidateAsync(UnformalizedDocument entity)
        {
            var validationResult = new ValidationResult();
            validationResult.Errors.AddRange(ValidateReference(entity));
            validationResult.ThrowIfAny();

            if (await unitOfWork.Set<UnformalizedDocument>()
                .AnyAsync(d => d.Id == entity.Id &&
                               d.IsSent))
            {
                throw new ForbiddenException("document-cannot-be-changed",
                    "Cannot change unformalized document when it is already sent");
            }
        }

        public async Task ValidateRequestedDocumentPermission(Guid id, Guid companyId)
        {
            if (!await unitOfWork.Set<UnformalizedDocument>()
                .AnyAsync(d => d.Id == id &&
                               (d.SenderId == companyId ||
                                (d.IsSent &&
                                 d.Receivers.Any(r => r.ReceiverId == companyId)))))
            {
                throw new NotFoundException();
            }
        }

        public async Task ValidateBeforeDeclining(UnformalizedDocument entity, Guid companyId)
        {
            switch (entity.Status)
            {
                case UnformalizedDocumentStatus.SignedByAll:
                    throw new ForbiddenException("signed-document-cannot-be-declined",
                        "Cannot decline unformalized document when it is already signed by all sides");
                case UnformalizedDocumentStatus.Declined:
                    throw new ForbiddenException("declined-document-cannot-be-declined",
                        "Cannot decline unformalized document when it is already declined");
            }

            if (entity.SenderId != companyId &&
                await unitOfWork.Set<UnformalizedDocument>()
                    .AnyAsync(d => d.Id == entity.Id &&
                        d.Receivers.All(r => r.ReceiverId != companyId)))
            {
                throw new ForbiddenException("decliner-is-not-sender-or-receiver",
                    "Only senders and receivers can decline the document");
            }
        }

        public async Task ValidateBeforeDeleteAsync(Guid id, Guid companyId)
        {
            if (await unitOfWork.Set<UnformalizedDocument>()
                .AnyAsync(d => d.Id == id &&
                               (d.IsSent ||
                                d.SenderId != companyId)))
            {
                throw new ForbiddenException("cannot-delete-document-is-not-draft",
                    "You are allowed to delete only draft documents");
            }
        }

        private IEnumerable<ValidationError> ValidateReference(UnformalizedDocument entity)
        {
            yield return referenceValidator
                .ValidateReference<UnformalizedDocument, Company>(d => d.SenderId,
                    entity,
                    null, nameof(entity.SenderId));
            foreach (var receiver in entity.Receivers)
            {
                yield return referenceValidator
                    .ValidateReference<UnformalizedDocumentReceiver, Company>(d => d.ReceiverId,
                        receiver,
                        null, nameof(receiver.ReceiverId));

                if (receiver.ReceiverId == entity.SenderId)
                {
                    yield return new ValidationError(nameof(receiver.ReceiverId),
                        new ForbiddenErrorDetails("sender-is-in-receivers"));
                }
            }
        }
    }
}