using System;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class ResidentPassportInfoSeed : ISeedDataStrategy<ResidentPassportInfo>
    {
        public ResidentPassportInfo[] GetSeedData()
        {
            return new[]
            {
                new ResidentPassportInfo
                {
                    CompanyId = GuidValues.CompanyGuids.TestSimpleUser,
                    PositionType = CompanyPositionType.Owner,
                    Date = DateTime.UtcNow.AddDays(-1),
                    Number = "numb",
                    Series = "ser",
                    UnitCode = "ucode",
                    SNILS = "test",
                    IssuingAuthorityPSRN = "test"
                }
            };
        }
    }
}