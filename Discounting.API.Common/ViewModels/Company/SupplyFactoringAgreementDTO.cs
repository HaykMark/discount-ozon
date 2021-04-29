using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.CompanyAggregates;

namespace Discounting.API.Common.ViewModels.Company
{
    public class SupplyFactoringAgreementDTO : DTO<Guid>
    {
        [CustomRequired]
        public Guid FactoringAgreementId { get; set; }
        [CustomRequired, CustomStringLength(150)]
        public string Number { get; set; }
        [CustomRequired]
        public DateTime Date { get; set; }
        public SupplyFactoringAgreementStatus Status { get; set; }
    }
}