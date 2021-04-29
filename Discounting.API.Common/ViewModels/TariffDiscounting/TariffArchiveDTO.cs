using System;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.TariffDiscounting
{
    public class TariffArchiveDTO : DTO<Guid>
    {
        public decimal FromAmount { get; set; }
        public decimal? UntilAmount { get; set; }
        public int FromDay { get; set; }
        public int? UntilDay { get; set; }
        public decimal Rate { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime ActionTime { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
    }
}