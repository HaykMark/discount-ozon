using System;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels.Common
{
    public class DeactivationDTO
    {
        [CustomStringLength(2000)]
        public string DeactivationReason { get; set; }
    }
}