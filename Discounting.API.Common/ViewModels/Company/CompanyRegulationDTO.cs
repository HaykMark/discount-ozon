using System;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Common.Types;
using Discounting.Entities.CompanyAggregates;

namespace Discounting.API.Common.ViewModels.Company
{
    public class CompanyRegulationDTO : DTO<Guid>
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public CompanyRegulationType Type { get; set; }
        public DateTime CreatedDate { get; set; }
        
        public Location Location => new Location {
            pathname = $@"/{Routes.CompanyRegulations}/{Routes.FileSubRoute}"
                .Replace(Routes.DetailSubRoute, this.Id.ToString())
        };
    }
}