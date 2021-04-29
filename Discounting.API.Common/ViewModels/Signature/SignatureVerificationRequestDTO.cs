using Discounting.Entities;

namespace Discounting.API.Common.ViewModels.Signature
{
    public class SignatureVerificationRequestDTO
    {
        public string Original { get; set; }
        public string Signature { get; set; }
        public SignatureType Type { get; set; }
    }
}