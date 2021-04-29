using System;
using Discounting.API.Common.Constants;
using Discounting.Common.Types;

namespace Discounting.API.Common.ViewModels.Signature
{
    public class RegistrySignatureDTO : SignatureDTO
    {
        public Guid RegistryId { get; set; }
        
        public override Location Location => new Location {
            pathname = $@"/{Routes.Registries}/{Routes.SignatureFileSubRoute}"
                .Replace(Routes.DetailSubRoute, this.RegistryId.ToString())
                .Replace(Routes.DetailSignatureFileSubRoute, this.Id.ToString())
        };
    }
}