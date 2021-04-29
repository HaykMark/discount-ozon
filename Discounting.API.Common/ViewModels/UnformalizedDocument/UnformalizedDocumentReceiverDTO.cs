using System;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.UnformalizedDocument
{
    public class UnformalizedDocumentReceiverDTO : DTO<Guid>
    {
        public Guid ReceiverId { get; set; }
        public bool NeedSignature { get; set; }
        public bool IsSigned { get; set; }
        public Guid UnformalizedDocumentId { get; set; }
    }
}