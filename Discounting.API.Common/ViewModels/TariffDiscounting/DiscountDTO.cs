using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.TariffDiscounting;

namespace Discounting.API.Common.ViewModels.TariffDiscounting
{
    public class DiscountDTO : DTO<Guid>
    {
        [CustomRequired]
        public DateTime PlannedPaymentDate { get; set; }
        [CustomRequired]
        [DecimalRange]
        public decimal AmountToPay { get; set; }
        [CustomRequired]
        [DecimalRange]
        public decimal DiscountedAmount { get; set; }
        [CustomRequired]
        [DecimalRange(0, 100)]
        public decimal Rate { get; set; }
        [CustomRequired]
        public DiscountingSource DiscountingSource { get; set; }
        [CustomRequired]
        public Guid RegistryId { get; set; }
        public bool HasChanged { get; set; }
    }
}