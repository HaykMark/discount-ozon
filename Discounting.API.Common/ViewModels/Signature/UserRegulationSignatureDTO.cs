using System;
using Discounting.API.Common.Constants;
using Discounting.Common.Types;

namespace Discounting.API.Common.ViewModels.Signature
{
    public class UserRegulationSignatureDTO : SignatureDTO
    {
        public Guid UserRegulationId { get; set; }

        public override Location Location => new Location
        {
            pathname = $@"/{Routes.UserRegulations}/{Routes.SignatureFileSubRoute}"
                .Replace(Routes.DetailSubRoute, this.UserRegulationId.ToString())
                .Replace(Routes.DetailSignatureFileSubRoute, this.Id.ToString())
        };
    }
}