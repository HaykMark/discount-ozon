using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures.CompanyAggregates
{
    public class MigrationCardFixture : BaseFixture
    {
        public MigrationCardFixture(AppState appState) : base(appState)
        {
        }
        
        public async Task<MigrationCardInfoDTO> CreateTestDataAsync(
            MigrationCardInfoDTO payload = null)
        {
            payload ??= GetPayload(GuidValues.CompanyGuids.TestSeller);
            
            return await MigrationCardDataApi.Create(payload.CompanyId, payload);
        }

        public MigrationCardInfoDTO GetPayload(Guid companyId)
        {
            return new MigrationCardInfoDTO
            {
                CompanyId = companyId,
                Address = "test",
                Phone = "123456789",
                PositionType = CompanyPositionType.AuthorizedUser,
                RegistrationAddress = "test",
                RightToResideDocument = "test"
            };
        }
    }
}