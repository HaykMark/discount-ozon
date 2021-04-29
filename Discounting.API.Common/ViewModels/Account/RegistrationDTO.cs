using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;
using Discounting.Entities;
using Discounting.Entities.CompanyAggregates;

namespace Discounting.API.Common.ViewModels.Account
{
    public class RegistrationDTO
    {

        [CustomRequired, CustomStringLength(150)]
        public string FirstName { get; set; }

        [CustomRequired, CustomStringLength(150)]
        public string SecondName { get; set; }

        [CustomRequired, CustomStringLength(150)]
        public string Surname { get; set; }

        [CustomRequired, CustomStringLength(50, MinimumLength = 6)]
        [EmailAddress]
        public string Email { get; set; }

        [CustomRequired, CustomStringLength(500)]
        public string FullName { get; set; }

        [CustomRequired, CustomStringLength(500)]
        public string ShortName { get; set; }

        [CustomStringLength(11)]
        public string Phone { get; set; }

        [CustomRequired, CustomStringLength(12)]
        public string TIN { get; set; }
        
        [CustomStringLength(12)]
        public string KPP { get; set; }

        [CustomRequired, CustomStringLength(32, MinimumLength = 10)]
        [CustomRegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{10,}$")]
        public string Password { get; set; }

        [CustomRequired, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        public CompanyType Type { get; set; }
    }
}