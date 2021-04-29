using System;
using Discounting.Entities.Account;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class UserRoleSeed : ISeedDataStrategy<UserRole>
    {
        public UserRole[] GetSeedData()
        {
            return new[]
            {
                new UserRole
                {
                    Id = Guid.NewGuid(), 
                    RoleId = GuidValues.RoleGuids.AdminRole, 
                    UserId = GuidValues.UserGuids.Admin
                },
                new UserRole
                {
                    Id = Guid.NewGuid(), 
                    RoleId = GuidValues.RoleGuids.AdminRole, 
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new UserRole
                {
                    Id = Guid.NewGuid(), 
                    RoleId = GuidValues.RoleGuids.AdminRole, 
                    UserId = GuidValues.UserGuids.TestBuyer
                },
                new UserRole
                {
                    Id = Guid.NewGuid(), 
                    RoleId = GuidValues.RoleGuids.SimpleUser, 
                    UserId = GuidValues.UserGuids.TestSimpleUser
                },
                new UserRole
                {
                    Id = Guid.NewGuid(), 
                    RoleId = GuidValues.RoleGuids.BankUser, 
                    UserId = GuidValues.UserGuids.BankUserOne
                },
                new UserRole
                {
                    Id = Guid.NewGuid(), 
                    RoleId = GuidValues.RoleGuids.BankUser, 
                    UserId = GuidValues.UserGuids.BankUserSecond
                }
            };
        }
    }
}