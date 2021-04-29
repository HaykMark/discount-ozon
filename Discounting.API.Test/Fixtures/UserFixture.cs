using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class UserFixture : BaseFixture
    {
        public UserFixture(AppState appState = null) : base(appState)
        {
        }

        public Task<UserDTO> CreateTestUserAsync(UserDTO payload = null)
        {
            payload ??= GetPayload();
            return UserApi.Create(payload);
        }

        public UserDTO GetPayload()
        {
            return new UserDTO
            {
                Email = "test1@email.com",
                FirstName = "Test",
                SecondName = "Testi",
                Surname = "Testyan",
                IsActive = false,
                IsSuperAdmin = false,
                CompanyId = GuidValues.CompanyGuids.TestSimpleUser
            };
        }
    }
}