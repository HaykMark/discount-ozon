using System;
using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities;

namespace Discounting.API.Common.ViewModels
{
    public class SupplyDTO : DTO<Guid>
    {
        [CustomRequired, CustomStringLength(150)]
        public string Number { get; set; }
        [CustomRequired]
        public DateTime Date { get; set; }
        public SupplyType Type { get; set; }
        [CustomRequired, DecimalRange]
        public decimal Amount{ get; set; }
        public Guid ContractId { get; set; }
        public Guid? BaseDocumentId { get; set; }
        [CustomStringLength(150)]
        public string BaseDocumentNumber { get; set; }
        public SupplyType BaseDocumentType { get; set; }
        public DateTime? BaseDocumentDate { get; set; }
        [CustomRequired, CustomStringLength(150)]
        public string ContractNumber { get; set; }
        [CustomRequired]
        public DateTime ContractDate { get; set; }
        public DateTime DelayEndDate { get; set; }
        public DateTime CreationDate { get; set; }
        public Guid SupplyId { get; set; }
        public Guid? RegistryId { get; set; }
        public SupplyProvider Provider { get; set; }
        public SupplyStatus Status { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool HasVerification { get; set; }
        [CustomRequired, Tin]
        public string SellerTin { get; set; }
        [CustomRequired, Tin]
        public string BuyerTin { get; set; }
        public Guid? BankId { get; set; }
        public bool AddedBySeller { get; set; }
        public bool SellerVerified { get; set; }
        public bool BuyerVerified { get; set; }
    }
}