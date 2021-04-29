using System;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.Signature
{
    public class SignatureInfoDTO : DTO<Guid>
    {
        public string Serial { get; set; }
        public string Thumbprint { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTill { get; set; }
        public string Company { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string INN { get; set; }
        public string OGRN { get; set; }
        public string SNILS { get; set; }
        public Guid SignatureId { get; set; }
    }
}