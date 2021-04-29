using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.EmailNotification;
using Discounting.API.Common.ViewModels.UnformalizedDocument;
using Discounting.Entities;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface IUnformalizedDocumentApi
    {
        [Get("/api/unformalized-documents")]
        Task<List<UnformalizedDocumentDTO>> GetAll(
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/unformalized-documents/sent")]
        Task<List<UnformalizedDocumentDTO>> GetSent(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? sentDateFrom = null,
            DateTime? sentDateTo = null,
            Guid? receiverId = null
        );

        [Get("/api/unformalized-documents/received")]
        Task<List<UnformalizedDocumentDTO>> GetReceived(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? receivedDateFrom = null,
            DateTime? receivedDateTo = null,
            Guid? senderId = null
        );

        [Post("/api/unformalized-documents/decline")]
        Task<UnformalizedDocumentDTO> Decline(UnformalizedDocumentDeclineDTO dto);

        [Get("/api/unformalized-documents/{id}")]
        Task<UnformalizedDocumentDTO> Get(Guid id);

        [Post("/api/unformalized-documents/send-email-notification")]
        Task<NoContentResult> SendEmail(UnformalizedDocumentEmailNotificationDTO dto);

        [Post("/api/unformalized-documents")]
        Task<UnformalizedDocumentDTO> Post(UnformalizedDocumentDTO dto);

        [Put("/api/unformalized-documents/{id}")]
        Task<UnformalizedDocumentDTO> Put(Guid id, UnformalizedDocumentDTO dto);

        [Delete("/api/unformalized-documents/{id}")]
        Task<NoContentResult> Delete(Guid id);
    }
}