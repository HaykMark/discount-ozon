using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.Company
{
    public class CompanyContactInfoDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string Address { get; set; }
        [CustomRequired, CustomStringLength(500)]
        public string OrganizationAddress { get; set; }
        [CustomRequired, CustomStringLength(11)]
        public string Phone { get; set; }
        [CustomRequired, CustomStringLength(50), CustomEmail]
        public string Email { get; set; }
        [CustomStringLength(50)]
        public string MailingAddress { get; set; }
        [CustomRequired, CustomStringLength(50)]
        public string NameOfGoverningBodies { get; set; }
    }
}