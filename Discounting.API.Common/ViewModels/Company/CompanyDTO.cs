using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.CompanyAggregates;

namespace Discounting.API.Common.ViewModels.Company
{
    public class CompanyDTO : DTO<Guid>
    {
        [CustomRequired, CustomStringLength(500)]
        public string FullName { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string ShortName { get; set; }
        [CustomRequired, CustomStringLength(12)]
        public string TIN { get; set; }
        [CustomStringLength(12)]
        public string KPP { get; set; }
        [CustomStringLength(300)]
        public string OwnerFullName { get; set; }
        [CustomStringLength(300)]
        public string OwnerPosition { get; set; }
        [CustomStringLength(250)]
        public string OwnerDocument { get; set; }
        [CustomRequired, CustomStringLength(15)]
        public string PSRN { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string RegisteringAuthorityName { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string RegistrationStatePlace { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string StateStatisticsCode { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string PaidUpAuthorizedCapitalInformation { get; set; }
        [CustomRequired, DateInThePast]
        public DateTime StateRegistrationDate { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string IncorporationForm { get; set; }

        public bool IsActive { get; set; }

        public bool HasPowerOfAttorney { get; set; }
        [CustomStringLength(2000)]
        public string DeactivationReason { get; set; }

        public CompanyType CompanyType { get; set; }
    }
}