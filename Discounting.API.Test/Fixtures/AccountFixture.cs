using Discounting.API.Common.ViewModels.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class AccountFixture : BaseFixture
    {
        public AccountFixture(AppState appState) : base(appState)
        {
        }

        public RegistrationDTO GetRegistrationPayload()
        {
            return new RegistrationDTO
            {
                Email = "test@test.com",
                Password = "zaq1@WSX12",
                ConfirmPassword = "zaq1@WSX12",
                Surname = "test",
                Type = CompanyType.SellerBuyer,
                FirstName = "test",
                FullName = "test company full name",
                ShortName = "test company short name",
                SecondName = "test",
                TIN = "6666666666"
            };
        }
    }
}