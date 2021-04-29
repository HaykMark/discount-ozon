using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures.CompanyAggregates
{
    public class CompanyContactFixture : BaseFixture
    {
        public CompanyContactFixture(AppState appState) : base(appState)
        {
        }

        public async Task<CompanyContactInfoDTO> CreateTestDataAsync(
            CompanyContactInfoDTO payload = null)
        {
            payload ??= GetPayload(GuidValues.CompanyGuids.TestSeller);

            return await CompanyContactApi.Create(payload.CompanyId, payload);
        }

        public CompanyContactInfoDTO GetPayload(Guid companyId)
        {
            return new CompanyContactInfoDTO
            {
                CompanyId = companyId,
                Address = "test",
                Email = "tt@tt.tt",
                Phone = "123456789",
                MailingAddress = "test",
                OrganizationAddress = "test",
                NameOfGoverningBodies = "test"
            };
        }
    }
}