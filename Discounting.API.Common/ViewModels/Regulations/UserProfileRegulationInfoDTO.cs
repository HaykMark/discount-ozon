using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.Regulations
{
    public class UserProfileRegulationInfoDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid UserRegulationId { get; set; }
        [CustomRequired]
        public DateTime Date { get; set; }
        [CustomStringLength(500)]
        public string Number { get; set; }
        [CustomRequired]
        public string Citizenship { get; set; }
        [CustomRequired]
        public string PlaceOfBirth { get; set; }
        [CustomRequired]
        public DateTime DateOfBirth { get; set; }
        public DateTime? AuthorityValidityDate { get; set; }
        [CustomRequired]
        public string IdentityDocument { get; set; }
        public bool IsResident { get; set; }

        //For residents
        public string PassportSeries { get; set; }
        public DateTime? PassportDate { get; set; }
        public string PassportNumber { get; set; }
        public string PassportUnitCode { get; set; }
        public string PassportIssuingAuthorityPSRN { get; set; }
        public string PassportSNILS { get; set; }

        //For non residents
        public string MigrationCardRightToResideDocument { get; set; }
        public string MigrationCardAddress { get; set; }
        public string MigrationCardRegistrationAddress { get; set; }
        public string MigrationCardPhone { get; set; }
    }
}