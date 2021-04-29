using System;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Common.Types;
using Discounting.Entities.Templates;

namespace Discounting.API.Common.ViewModels.Templates
{
    public class TemplateDTO : DTO<Guid>
    {
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public TemplateType Type { get; set; }
        public DateTime CreatedDate { get; set; }
        
        public Location Location => new Location {
            pathname = $@"/{Routes.Templates}/{Routes.FileSubRoute}"
                .Replace(Routes.DetailSubRoute, this.Id.ToString())
        };
    }
}