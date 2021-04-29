using System;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.Common.Types;

namespace Discounting.API.Common.ViewModels.Company
{
    public class CompanyRegulationSignatureDTO : SignatureDTO
    {
        public Guid CompanyRegulationId { get; set; }
        public override Location Location => new Location {
            pathname = $@"/{Routes.CompanyRegulations}/{Routes.SignatureFileSubRoute}"
                .Replace(Routes.DetailSubRoute, this.CompanyRegulationId.ToString())
                .Replace(Routes.DetailSignatureFileSubRoute, this.Id.ToString())
        };
    }
}