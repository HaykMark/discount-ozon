using System;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class CompanyOwnerPositionInfoSeed : ISeedDataStrategy<CompanyOwnerPositionInfo>
    {
        public CompanyOwnerPositionInfo[] GetSeedData()
        {
            return new[]
            {
                new CompanyOwnerPositionInfo
                {
                    Name = "test Name",
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
                    IsResident = true,
                    AuthorityValidityDate = DateTime.UtcNow.AddDays(1)
                }
            };
        }
    }
}