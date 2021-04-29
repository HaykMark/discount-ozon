using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Common.AccessControl;
using Discounting.Common.Types;
using Discounting.Entities.Account;

namespace Discounting.API.Common.ViewModels.Account
{
    /// <summary>
    /// Data sent over the network for user's role.
    /// </summary>
    public class RoleDTO : DTO<Guid>, IWithSystemFlag
    {
        [CustomRequired]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool IsSystemDefault { get; set; }
        public RoleType Type { get; set; }

        public Dictionary<string, Operations> Permissions { get; set; }
    }
}