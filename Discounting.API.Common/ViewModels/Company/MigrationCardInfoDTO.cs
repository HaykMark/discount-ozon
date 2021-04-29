using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.CompanyAggregates;

namespace Discounting.API.Common.ViewModels.Company
{
    public class MigrationCardInfoDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string RightToResideDocument { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string Address { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string RegistrationAddress { get; set; }
        [CustomRequired, CustomStringLength(11)]
        public string Phone { get; set; }
        [CustomRequired]
        public CompanyPositionType PositionType { get; set; }
    }
}