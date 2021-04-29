using System;
using Discounting.API.Common.Constants;
using Discounting.Common.Types;

namespace Discounting.API.Common.ViewModels.Signature
{
    public class UnformalizedDocumentSignatureDTO : SignatureDTO
    {
        public Guid UnformalizedDocumentId { get; set; }
        
        public override Location Location => new Location {
            pathname = $@"/{Routes.UnformalizedDocuments}/{Routes.SignatureFileSubRoute}"
                .Replace(Routes.DetailSubRoute, this.UnformalizedDocumentId.ToString())
                .Replace(Routes.DetailSignatureFileSubRoute, this.Id.ToString())
        };
    }
}