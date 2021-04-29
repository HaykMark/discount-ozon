using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities;

namespace Discounting.API.Common.ViewModels.UnformalizedDocument
{
    public class UnformalizedDocumentDTO : DTO<Guid>
    {
        public Guid SenderId { get; set; }
        [CustomRequired]
        public UnformalizedDocumentType Type { get; set; }
        [CustomRequired, CustomStringLength(100)]
        public string Topic { get; set; }
        [CustomStringLength(1000)]
        public string Message { get; set; }
        public UnformalizedDocumentStatus Status { get; set; }
        public bool IsSent { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? SentDate { get; set; }
        public Guid? DeclinedBy { get; set; }
        public DateTime? DeclinedDate { get; set; }
        [CustomStringLength(1000)]
        public string DeclineReason { get; set; }
        [CustomRequired]
        public UnformalizedDocumentReceiverDTO[] Receivers { get; set; }
    }
}