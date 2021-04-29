using System;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels.Account
{
    public class UserActivationDTO
    {
        public string BaseUrl { get; set; }

        [CustomRequired]
        public Guid UserId { get; set; }
    }
}