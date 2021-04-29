using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels.Account
{
    public class PasswordDTO
    {
        [CustomRequired]
        [CustomStringLength(32, MinimumLength = 10)]
        [CustomRegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{10,}$")]
        [CustomDataType(DataType.Password)]
        public string Password { get; set; }
    }
}