using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.UnformalizedDocument
{
    public class UnformalizedDocumentDeclineDTO : DTO<Guid>
    {
        [CustomStringLength(1000)]
        public string DeclineReason { get; set; }
    }
}