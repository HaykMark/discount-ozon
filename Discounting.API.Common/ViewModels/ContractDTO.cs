using System;
using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities;

namespace Discounting.API.Common.ViewModels
{
    public class ContractDTO : DTO<Guid>
    {
        public Guid SellerId { get; set; }
        public Guid BuyerId { get; set; }
        public ContractStatus Status { get; set; }
        public Guid? CreatorId { get; set; }
        [CustomDataType(DataType.Date)]
        public DateTime CreationDate { get; set; }
        [CustomDataType(DataType.Date)]
        public DateTime? UpdateDate { get; set; }
        public ContractProvider Provider { get; set; }
        public bool IsRequiredRegistry { get; set; }
        public bool IsRequiredNotification { get; set; }
        [CustomRequired, Tin]
        public string SellerTin { get; set; }
        public bool IsDynamicDiscounting { get; set; }
        public bool IsFactoring { get; set; }
    }
}