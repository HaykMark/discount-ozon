using System;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.TariffDiscounting;

namespace Discounting.API.Common.ViewModels.TariffDiscounting
{
    public class SupplyDiscountDTO : DTO<Guid>
    {
        public decimal Rate { get; set; }
        public decimal DiscountedAmount { get; set; }
        public Guid SupplyId { get; set; }
    }
}