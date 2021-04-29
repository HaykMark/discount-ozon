using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures.CompanyAggregates
{
    public class CompanyAuthorizedUserFixture : BaseFixture
    {
        public CompanyAuthorizedUserFixture(AppState appState) : base(appState)
        {
        }

        public async Task<CompanyAuthorizedUserInfoDTO> CreateTestDataAsync(
            CompanyAuthorizedUserInfoDTO payload = null)
        {
            payload ??= GetPayload(GuidValues.CompanyGuids.TestSeller);

            return await CompanyAuthorizedUserApi.Create(payload.CompanyId, payload);
        }

        public CompanyAuthorizedUserInfoDTO GetPayload(Guid companyId)
        {
            return new CompanyAuthorizedUserInfoDTO
            {
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
                IsResident = false,
                AuthorityValidityDate = DateTime.UtcNow.AddDays(1)
            };
        }
    }
}