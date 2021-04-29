using System;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels
{
    public class FreeDayDTO : DTO<Guid>
    {
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }
}