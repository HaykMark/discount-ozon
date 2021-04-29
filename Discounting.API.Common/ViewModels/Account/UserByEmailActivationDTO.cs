using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels.Account
{
    public class UserByEmailActivationDTO
    {
        public string BaseUrl { get; set; }
        [CustomRequired]
        [EmailAddress]
        public string Email { get; set; }
    }
}