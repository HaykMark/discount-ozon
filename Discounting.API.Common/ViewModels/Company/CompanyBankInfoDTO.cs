using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.Company
{
    public class CompanyBankInfoDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired, CustomStringLength(250)]
        public string Name { get; set; }
        [CustomRequired, CustomStringLength(250)]
        public string City { get; set; }
        [CustomRequired, CustomStringLength(9, MinimumLength = 9), Number]
        public string Bic { get; set; }
        [CustomRequired, CustomStringLength(13, MinimumLength = 13), Number]
        public string OGRN { get; set; }
        [CustomRequired, CustomStringLength(20, MinimumLength = 20), Number]
        public string CorrespondentAccount { get; set; }
        [CustomRequired, CustomStringLength(20, MinimumLength = 20), Number]
        public string CheckingAccount { get; set; }
        public bool IsActive { get; set; }
    }
}