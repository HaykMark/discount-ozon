using System;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.Templates;

namespace Discounting.API.Common.ViewModels.Templates
{
    public class TemplateLookupsDTO : DTO<Guid>
    {
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public TemplateType Type { get; set; }
    }
}