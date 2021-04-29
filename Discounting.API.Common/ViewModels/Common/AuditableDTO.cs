using System;

namespace Discounting.API.Common.ViewModels.Common
{
    public class AuditableDTO<T> : DTO<T>
    {
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}