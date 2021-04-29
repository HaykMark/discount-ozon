using System;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels.Account
{
    public class UserConfirmationCodeDTO
    {
        [CustomRequired]
        public Guid UserId { get; set; }

        [CustomRequired]
        public string Code { get; set; }
    }
}