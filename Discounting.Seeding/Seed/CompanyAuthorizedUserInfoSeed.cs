using System;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class CompanyAuthorizedUserInfoSeed : ISeedDataStrategy<CompanyAuthorizedUserInfo>
    {
        public CompanyAuthorizedUserInfo[] GetSeedData()
        {
            return new[]
            {
                new CompanyAuthorizedUserInfo
                {
                    Citizenship = "test Citizenship",
                    Number = "test Number",
                    FirstName = "test FirstName",
                    SecondName = "test SecondName",
                    LastName = "test LastName",
                    PlaceOfBirth = "test PlaceOfBirth",
                    IdentityDocument = "test IdentityDocument",
                    Date = DateTime.UtcNow.AddDays(-1),
                    CompanyId = GuidValues.CompanyGuids.TestSimpleUser,
                    DateOfBirth = DateTime.UtcNow.AddYears(-1),
                    IsResident = false,
                    AuthorityValidityDate = DateTime.UtcNow.AddDays(1)
                }
            };
        }
    }
}