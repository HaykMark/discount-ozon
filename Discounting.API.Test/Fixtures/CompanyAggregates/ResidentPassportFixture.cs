using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures.CompanyAggregates
{
    public class ResidentPassportFixture : BaseFixture
    {
        public ResidentPassportFixture(AppState appState) : base(appState)
        {
        }
        
        public async Task<ResidentPassportInfoDTO> CreateTestDataAsync(
            ResidentPassportInfoDTO payload = null)
        {
            payload ??= GetPayload(GuidValues.CompanyGuids.TestSeller);

            return await ResidentPassportApi.Create(payload.CompanyId, payload);
        }

        public ResidentPassportInfoDTO GetPayload(Guid companyId)
        {
            return new ResidentPassportInfoDTO
            {
                CompanyId = companyId,
                PositionType = CompanyPositionType.Owner,
                Date = DateTime.UtcNow.AddDays(-1),
                Number = "test",
                Series = "test",
                UnitCode = "test",
                SNILS = "test",
                IssuingAuthorityPSRN = "test"
            };
        }
    }
}