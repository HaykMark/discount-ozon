using System;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.Regulations;

namespace Discounting.API.Common.ViewModels.Regulations
{
    public class RegulationDTO : DTO<Guid>
    {
        public RegulationType Type { get; set; }
    }
}