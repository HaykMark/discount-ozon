using System;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.Company
{
    public class CompanySettingsDTO : DTO<Guid>
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public Guid? DefaultTariff { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsSendAutomatically { get; set; }
        public bool IsRequiredRegistry { get; set; }
        public bool IsAuction { get; set; }
        public bool ForbidSellerEditTariff { get; set; }
    }
}