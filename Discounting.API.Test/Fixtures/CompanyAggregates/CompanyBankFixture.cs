using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures.CompanyAggregates
{
    public class CompanyBankFixture : BaseFixture
    {
        public CompanyBankFixture(AppState appState) : base(appState)
        {
        }
        
        public async Task<CompanyBankInfoDTO> CreateTestDataAsync(
            CompanyBankInfoDTO payload = null)
        {
            payload ??= GetPayload(GuidValues.CompanyGuids.TestSeller);

            return await CompanyBankApi.Create(payload.CompanyId, payload);
        }

        public CompanyBankInfoDTO GetPayload(Guid companyId)
        {
            return new CompanyBankInfoDTO()
            {
                CompanyId = companyId,
                Name = "test",
                City = "test",
                Bic = "123456789",
                OGRN = "1234567890123",
                CheckingAccount = "12345678901234567890",
                CorrespondentAccount = "09876543211234567890"
            };
        }
    }
}