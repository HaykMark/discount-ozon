using System.Collections.Generic;

namespace Discounting.API.Common.ViewModels.Account
{
    /// <summary>
    /// Data transfer object (DTO) defines how the data will be sent over the network for user session data.
    /// </summary>
    public class SessionInfoDTO
    {
        /// <summary>
        /// Application user data.
        /// </summary>
        public UserDTO User { get; set; }
        /// <summary>
        /// Auth token resource data.
        /// </summary>
        public TokenDTO TokenResource { get; set; }
        /// <summary>
        /// User roles
        /// </summary>
        public List<RoleDTO> Roles { get; set; }
    }
}