using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.Templates
{
    public class BuyerTemplateConnectionDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid BuyerId { get; set; }
        [CustomRequired]
        public Guid TemplateId { get; set; }

        public string BuyerName { get; set; }
        public string TemplateName { get; set; }
    }
}