using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.CompanyAggregates;

namespace Discounting.API.Common.ViewModels.Company
{
    public class ResidentPassportInfoDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired, CustomStringLength(4)]
        public string Series { get; set; }
        [CustomRequired, DateInThePast]
        public DateTime Date { get; set; }
        [CustomRequired, CustomStringLength(6)]
        public string Number { get; set; }
        [CustomRequired, CustomStringLength(7)]
        public string UnitCode { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string IssuingAuthorityPSRN { get; set; }
        [CustomStringLength(500)]
        public string SNILS { get; set; }
        [CustomRequired]
        public CompanyPositionType PositionType { get; set; }
    }
}