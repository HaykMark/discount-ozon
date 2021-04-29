using System.Collections.Generic;
using System.Linq;
using Discounting.Common.AccessControl;

namespace Discounting.Entities.Account
{
    public class Permissions : List<Permission>
    {
        public Permissions() : base() { }
        public Permissions(IEnumerable<Permission> permissions) : base(permissions) { }

        /// <summary>
        /// Helper used to replace a set of permissions in the original set.
        /// </summary>
        /// <returns>a reference to the current set</returns>
        public Permissions Replace(IEnumerable<(string, Operations)> permissions)
        {
            var thisAsDict = this.ToDictionary(p => p.ZoneId);
            permissions.ToList().ForEach(p => {
                thisAsDict[p.Item1].Operations = p.Item2;
            });

            return this;
        }
    }
}