using System.Linq;
using Discounting.Common.AccessControl;
using Discounting.Entities.Account;
using Discounting.Seeding.Constants;
using static Discounting.Common.AccessControl.Operations;

namespace Discounting.Seeding.Seed
{
    public class PermissionSeed : ISeedDataStrategy<Permission>
    {
        public Permission[] GetSeedData()
        {
            var zoneIds = typeof(Zones).GetFields()
                .Select(f => (string) f.GetValue(null))
                .ToList();

            // Create admin permissions
            var adminPermissions = new Permissions();
            foreach (var zoneId in zoneIds)
            {
                adminPermissions.Add(new Permission
                {
                    Id = 1 + adminPermissions.Count(),
                    ZoneId = zoneId,
                    Operations = All,
                    RoleId = GuidValues.RoleGuids.AdminRole, // admin
                });
            }

            // Create user permissions
            var userPermissions = new Permissions();
            foreach (var zoneId in zoneIds)
                userPermissions.Add(new Permission
                {
                    Id = 1000 + userPermissions.Count,
                    ZoneId = zoneId,
                    Operations = All,
                    RoleId = GuidValues.RoleGuids.SimpleUser, // simple user
                });

            //TODO add right Bank Roles
            var bankPermissions = new Permissions();
            foreach (var zoneId in zoneIds)
            {
                bankPermissions.Add(new Permission
                {
                    Id = 2000 + bankPermissions.Count(),
                    ZoneId = zoneId,
                    Operations = All,
                    RoleId = GuidValues.RoleGuids.BankUser, // bank
                });
            }

            var initialPermissions = new Permissions();
            var initialPermissionZones = zoneIds.Where(z => z != null &&
                                                            (z.StartsWith("system") ||
                                                             z.StartsWith("companies")))
                .ToList();
            initialPermissionZones.Add(Zones.Signatures);
            foreach (var zone in initialPermissionZones)
            {
                if (zone.StartsWith("system"))
                {
                    initialPermissions.Add(new Permission
                    {
                        Id = 3000 + initialPermissions.Count(),
                        ZoneId = zone,
                        Operations = All,
                        RoleId = GuidValues.RoleGuids.InactiveCompanyRole
                    });
                }
                else
                {
                    initialPermissions.Add(new Permission
                    {
                        Id = 3000 + initialPermissions.Count,
                        ZoneId = zone,
                        Operations = All,
                        RoleId = GuidValues.RoleGuids.InactiveCompanyRole
                    });
                }
            }

            return userPermissions
                .Concat(adminPermissions)
                .Concat(bankPermissions)
                .Concat(initialPermissions)
                .ToArray();
        }
    }
}