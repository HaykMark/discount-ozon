using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.TariffDiscounting;

namespace Discounting.API.Common.ViewModels.TariffDiscounting
{
    public class TariffDTO : DTO<Guid>
    {
        [CustomRequired]
        [DecimalRange(1)]
        public decimal FromAmount { get; set; }
        public decimal? UntilAmount { get; set; }
        [CustomRequired]
        [CustomRange(1, 366)]
        public int FromDay { get; set; }
        public int? UntilDay { get; set; }
        [CustomRequired]
        [DecimalRange(0, 100)]
        public decimal Rate { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreationDate { get; set; }
        
        [CustomRequired]
        public TariffType Type { get; set; }
    }
}