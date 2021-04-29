using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Account;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Entities.CompanyAggregates;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures.CompanyAggregates
{
    public class CompanyFixture : BaseFixture
    {
        public CompanyFixture(AppState appState) : base(appState)
        {
        }

        public CompanyDTO GetPayload(CompanyType type = CompanyType.SellerBuyer)
        {
            return new CompanyDTO
            {
                CompanyType = type,
                ShortName = "test short",
                FullName = "test full",
                TIN = "6666666666",
                RegisteringAuthorityName = "test",
                RegistrationStatePlace = "test",
                StateRegistrationDate = DateTime.UtcNow.AddDays(-1),
                StateStatisticsCode = "test",
                PaidUpAuthorizedCapitalInformation = "test",
                IncorporationForm = "test",
                PSRN = "1234567"
            };
        }

        public Task<CompanyDTO> CreateCompanyAsync(CompanyDTO payload = null)
        {
            return CompanyApi.Post(payload ?? GetPayload());
        }

        public CompanySettingsDTO GetSettingsPayload(UserDTO user)
        {
            return new CompanySettingsDTO
            {
                CompanyId = user.CompanyId,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow
            };
        }
    }
}