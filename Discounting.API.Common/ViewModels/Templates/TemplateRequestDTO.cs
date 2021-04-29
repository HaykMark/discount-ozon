using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.Templates;
using Microsoft.AspNetCore.Http;

namespace Discounting.API.Common.ViewModels.Templates
{
    public class TemplateRequestDTO : DTO<Guid>
    {
        [CustomRequired]
        public TemplateType Type { get; set; }
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired] 
        [AllowedExtension]
        public IFormFile File { get; set; }
    }
}