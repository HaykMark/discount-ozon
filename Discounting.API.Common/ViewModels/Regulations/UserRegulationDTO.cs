using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.Regulations;

namespace Discounting.API.Common.ViewModels.Regulations
{
    public class UserRegulationDTO : DTO<Guid>
    {
        public UserRegulationType Type { get; set; }
        [CustomRequired]
        public Guid UserId { get; set; }

        //if UserRegulationType is profile than this field is required
        public UserProfileRegulationInfoDTO UserProfileRegulationInfo { get; set; }
    }
}