using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class MigrationCardInfoSeed : ISeedDataStrategy<MigrationCardInfo>
    {
        public MigrationCardInfo[] GetSeedData()
        {
            return new[]
            {
                new MigrationCardInfo
                {
                    CompanyId = GuidValues.CompanyGuids.TestSimpleUser,
                    Address = "test Address",
                    Phone = "123456789",
                    PositionType = CompanyPositionType.AuthorizedUser,
                    RegistrationAddress = "test RegistrationAddress",
                    RightToResideDocument = "test RightToResideDocument"
                }
            };
        }
    }
}