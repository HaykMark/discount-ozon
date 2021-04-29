using Discounting.Common.AccessControl;
using Discounting.Entities.Account;

namespace Discounting.Logics.AccessControl
{
    /// <summary>
    /// Checks whether the user has the necessary permissions to
    /// perform the `operations` in the given `zone`.
    /// </summary>
    public delegate bool PermissionChecker(Operations operations, string zoneId);

    public class FirewallContext
    {
        public User User { get; set; }
        public PermissionChecker HasPermission { get; set; }
    }
}
