using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class PasswordPolicyFixture : BaseFixture
    {
        public PasswordPolicyFixture(AppState appState = null) : base(appState)
        {
        }

        public PasswordDTO GetPayload(string newPassword)
        {
            return new PasswordDTO
            {
                Password = newPassword
            };
        }

        public async Task<UserDTO> SetupTestPasswordPolicyAsync()
        {
            var testUserDto = UserFixture.GetPayload();
            var responsePost = await UserApi.Create(testUserDto);
            return responsePost;
        }
    }
}