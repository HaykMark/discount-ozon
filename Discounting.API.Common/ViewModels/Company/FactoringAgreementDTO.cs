using System;
using System.Collections.Generic;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.CompanyAggregates;

namespace Discounting.API.Common.ViewModels.Company
{
    public class FactoringAgreementDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired]
        public Guid BankId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsConfirmed { get; set; }
        [CustomRequired, CustomStringLength(150)]
        public string FactoringContractNumber { get; set; }
        [CustomRequired]
        public DateTime? FactoringContractDate { get; set; }
        [CustomRequired, CustomStringLength(250)]
        public string BankName { get; set; }
        [CustomRequired, CustomStringLength(250)]
        public string BankCity { get; set; }
        [CustomRequired, CustomStringLength(9, MinimumLength = 9), Number]
        public string BankBic { get; set; }
        [CustomRequired, CustomStringLength(13, MinimumLength = 13), Number]
        public string BankOGRN { get; set; }
        [CustomRequired, CustomStringLength(20, MinimumLength = 20), Number]
        public string BankCorrespondentAccount { get; set; }
        [CustomRequired, CustomStringLength(20, MinimumLength = 20), Number]
        public string BankCheckingAccount { get; set; }
        
        public List<SupplyFactoringAgreementDTO> SupplyFactoringAgreementDtos { get; set; }
    }
}