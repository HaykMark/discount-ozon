using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels.Account
{
    /// <summary>
    /// Data sent over the network for login.
    /// </summary>
    public class LoginDTO
    {
        [CustomRequired]
        [CustomStringLength(100, MinimumLength = 4)]
        [CustomDataType(DataType.Password)]
        public string Password { get; set; }
        public bool? RememberMe { get; set; }

        [CustomRequired]
        [CustomEmail]
        [CustomStringLength(450)]
        public string UserIdentifier { get; set; }
    }
}