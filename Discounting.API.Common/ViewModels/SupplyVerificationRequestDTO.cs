using System;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels
{
    public class SupplyVerificationRequestDTO
    {
        [CustomRequired]
        public Guid[] SupplyIds { get; set; }
        [CustomRequired]
        public Guid BankId { get; set; }
        [CustomRequired]
        public Guid FactoringAgreementId { get; set; }
    }
}