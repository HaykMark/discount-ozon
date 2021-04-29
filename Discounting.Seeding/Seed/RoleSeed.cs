using Discounting.Entities.Account;
using Discounting.Helpers;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class RoleSeed : ISeedDataStrategy<Role>
    {
        public Role[] GetSeedData() =>
            new[]
            {
                new Role
                {
                    Id = GuidValues.RoleGuids.AdminRole,
                    Name = "Manager",
                    Description = "This role has all permissions activated.",
                    Type = RoleType.SuperAdmin, 
                    IsSystemDefault = true
                },
                new Role
                {
                    Id = GuidValues.RoleGuids.BankUser,
                    Name = "Bank",
                    Description = "This role has all bank permissions activated.",
                    Type = RoleType.Bank,
                    IsSystemDefault = true
                },
                new Role
                {
                    Id = GuidValues.RoleGuids.SimpleUser,
                    Name = "Simple User",
                    Description = @"
                        Users with this role can do everything except from
                        doing administrative tasks such as updating users or roles
                    ".AsOneLine(),
                    Type = RoleType.SellerBuyer,
                    IsSystemDefault = true
                },
                new Role
                {
                    Id = GuidValues.RoleGuids.InactiveCompanyRole,
                    Name = "Inactive Company",
                    Description = "Users with this role can only upload and sign regulations and also change password",
                    Type = RoleType.InactiveCompany,
                    IsSystemDefault = true
                }
            };
    }
}