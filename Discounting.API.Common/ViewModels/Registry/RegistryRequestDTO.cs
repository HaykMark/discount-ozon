using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.Entities;

namespace Discounting.API.Common.ViewModels.Registry
{
    public class RegistryRequestDTO
    {
        [CustomRequired]
        public Guid[] SupplyIds { get; set; }

        public FinanceType FinanceType { get; set; }
        
        public Guid? BankId { get; set; }
        public Guid? FactoringAgreementId { get; set; }
    }
}