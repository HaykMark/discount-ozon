using System;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Common.Types;
using Discounting.Entities;

namespace Discounting.API.Common.ViewModels.Signature
{
    public abstract class SignatureDTO : DTO<Guid>
    {
        public Guid SignerId { get; set; }
        public SignatureType Type { get; set; }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public SignatureInfoDTO Info { get; set; }

        public abstract Location Location { get; }
    }
}