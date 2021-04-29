using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities;

namespace Discounting.API.Common.ViewModels.Registry
{
    public class RegistryDTO : DTO<Guid>
    {
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public Guid ContractId { get; set; }
        public Guid CreatorId { get; set; }
        public RegistryStatus Status { get; set; }
        public RegistrySignStatus SignStatus { get; set; }
        public bool IsVerified { get; set; }
        public bool IsConfirmed { get; set; }
        [CustomStringLength(4000)]
        public string Remark { get; set; }
        public Guid? BankId { get; set; }
        public Guid? FactoringAgreementId { get; set; }
        public FinanceType FinanceType { get; set; }
    }
}