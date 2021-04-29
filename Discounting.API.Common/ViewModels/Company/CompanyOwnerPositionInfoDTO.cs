using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.Company
{
    public class CompanyOwnerPositionInfoDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string Name { get; set; }
        [CustomRequired, DateInThePast]
        public DateTime Date { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string Number { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string FirstName { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string SecondName { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string LastName { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string Citizenship { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string PlaceOfBirth { get; set; }
        [CustomRequired, DateInThePast]
        public DateTime DateOfBirth { get; set; }
        [CustomRequired]
        public DateTime AuthorityValidityDate { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string IdentityDocument { get; set; }
        public bool IsResident { get; set; }
    }
}