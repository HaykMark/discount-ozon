using System;
using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.Account
{
    /// <summary>
    /// Data transfer object (DTO) defines how the data will be sent over the network for User related data.
    /// </summary>
    public class UserDTO : DTO<Guid>
    {

        [CustomRequired, CustomStringLength(150)]
        public string FirstName { get; set; }
        
        [CustomStringLength(150)]
        public string SecondName { get; set; }

        [CustomRequired, CustomStringLength(150)]
        public string Surname { get; set; }
        
        [CustomStringLength(500)]
        public string Position { get; set; }

        [CustomRequired]
        public bool IsActive { get; set; }

        public bool IsSuperAdmin { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsConfirmedByAdmin { get; set; }
        
        [CustomStringLength(2000)]
        public string DeactivationReason { get; set; }

        [CustomRequired, CustomStringLength(50, MinimumLength = 6)]
        [EmailAddress]
        public string Email { get; set; }

        [CustomStringLength(11)]
        public string Phone { get; set; }

        public bool CanSign { get; set; }

        public Guid CompanyId { get; set; }

        public DateTimeOffset? LastLoggedIn { get; set; }
    }
}