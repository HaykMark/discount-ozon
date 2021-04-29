using System;

namespace Discounting.Common.Types
{
    public interface IMeta
    {
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}