using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class CompanyContactInfoSeed : ISeedDataStrategy<CompanyContactInfo>
    {
        public CompanyContactInfo[] GetSeedData()
        {
            return new[]
            {
                new CompanyContactInfo
                {
                    CompanyId = GuidValues.CompanyGuids.TestSimpleUser,
                    Address = "test Address",
                    Email = "tt@tt.tt",
                    Phone = "123456789",
                    MailingAddress = "test MailingAddress",
                    OrganizationAddress = "test OrganizationAddress",
                    NameOfGoverningBodies = "test NameOfGoverningBodies"
                }
            };
        }
    }
}