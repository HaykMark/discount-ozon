using System.Collections.Generic;
using Discounting.Common.Validation;

namespace Discounting.API.Common.ViewModels
{
    public class SupplyResponseDTO
    {
        public List<SupplyDTO> Supplies { get; set; }
        public ValidationErrors Errors { get; set; }

        public int ImportedCount { get; set; }
        public int FailedCount { get; set; }
    }
}