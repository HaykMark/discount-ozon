using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Account;
using Xunit;

namespace Discounting.Tests.Fixtures.Common
{
    public partial class BaseFixture
    {
        private TokenDTO token;

        public async Task<SessionInfoDTO> LoginAdminAsync(bool storeAccessToken = true)
        {
            return await LoginAsync("monitoringetp@yandex.ru", "1234", storeAccessToken);
        }

        public async Task<SessionInfoDTO> LoginSimpleUserAsync(bool storeAccessToken = true)
        {
            return await LoginAsync("client@discounting.ru", "1234", storeAccessToken);
        }

        public async Task<SessionInfoDTO> LoginSellerAsync(bool storeAccessToken = true)
        {
            return await LoginAsync("e.postavshik@yandex.ru", "1234", storeAccessToken);
        }

        public async Task<SessionInfoDTO> LoginBuyerAsync(bool storeAccessToken = true)
        {
            return await LoginAsync("zakazchik.etp@yandex.ru", "1234", storeAccessToken);
        }

        public async Task<SessionInfoDTO> LoginBankAsync(bool storeAccessToken = true)
        {
            return await LoginAsync("b.etp@yandex.ru", "1234", storeAccessToken);
        }

        public async Task<SessionInfoDTO> LoginSecondBankAsync(bool storeAccessToken = true)
        {
            return await LoginAsync("bank2@discounting.ru", "1234", storeAccessToken);
        }

        public async Task<SessionInfoDTO> LoginSuperAdminAsync(bool storeAccessToken = true)
        {
            return await LoginAsync("monitoringetp@yandex.ru", "1234", storeAccessToken);
        }

        public async Task<SessionInfoDTO> LoginAsync(string userIdentifier, string password,
            bool storeAccessToken = true)
        {
            var loginInfo = new LoginDTO
            {
                UserIdentifier = userIdentifier,
                Password = password
            };
            var sessionInfo = await AccountApi.Login(loginInfo);
            Assert.NotNull(sessionInfo);
            Assert.NotNull(sessionInfo.TokenResource);
            Assert.False(string.IsNullOrEmpty(sessionInfo.TokenResource.AccessToken));
            if (storeAccessToken) UseTokenForAuthorization(sessionInfo.TokenResource);

            return sessionInfo;
        }

        public void UseTokenForAuthorization(TokenDTO tokenDto)
        {
            token = tokenDto;

            HttpClient.DefaultRequestHeaders.Remove("Authorization");
            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.AccessToken}");
        }

        public async Task LogoutAsync()
        {
            if (token != null)
            {
                await AccountApi.Logout(token);
                token = null;
            }
        }
    }
}