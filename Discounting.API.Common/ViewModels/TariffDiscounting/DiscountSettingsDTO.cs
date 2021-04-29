using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.TariffDiscounting;

namespace Discounting.API.Common.ViewModels.TariffDiscounting
{
    public class DiscountSettingsDTO : AuditableDTO<Guid>
    {
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired, CustomRange(0, 366)]
        public int MinimumDaysToShift { get; set; }
        [CustomRequired]
        public DaysType DaysType { get; set; }
        [CustomRequired]
        public DaysOfWeek PaymentWeekDays { get; set; }
        
        
    }
}