using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures.CompanyAggregates
{
    public class CompanyOwnerPositionFixture : BaseFixture
    {
        public CompanyOwnerPositionFixture(AppState appState) : base(appState)
        {
        }

        public async Task<CompanyOwnerPositionInfoDTO> CreateTestDataAsync(
            CompanyOwnerPositionInfoDTO payload = null)
        {
            payload ??= GetPayload(GuidValues.CompanyGuids.TestSeller);

            return await CompanyOwnerPositionApi.Create(payload.CompanyId, payload);
        }

        public CompanyOwnerPositionInfoDTO GetPayload(Guid companyId)
        {
            return new CompanyOwnerPositionInfoDTO
            {
                Name = "test",
                Citizenship = "test",
                Number = "test",
                FirstName = "test",
                SecondName = "test",
                LastName = "test",
                PlaceOfBirth = "test",
                IdentityDocument = "test",
                Date = DateTime.UtcNow.AddDays(-1),
                CompanyId = companyId,
                DateOfBirth = DateTime.UtcNow.AddYears(-1),
                IsResident = true,
                AuthorityValidityDate = DateTime.UtcNow.AddDays(1)
            };
        }
    }
}