using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.CompanyAggregates;
using Microsoft.AspNetCore.Http;

namespace Discounting.API.Common.ViewModels.Company
{
    public class CompanyRegulationRequestDTO : DTO<Guid>
    {
        [CustomRequired] public CompanyRegulationType Type { get; set; }

        [CustomRequired]
        [AllowedExtension]
        public IFormFile File { get; set; }
    }
}