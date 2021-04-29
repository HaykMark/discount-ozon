using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests.Account
{
    public class AccountRefreshTokenTests : TestBase
    {
        public AccountRefreshTokenTests(AppState appState) : base(appState)
        {
            baseFixture = new BaseFixture(appState);
        }

        private readonly BaseFixture baseFixture;

        [Fact]
        public async Task Post_RefreshToken_Returns_SessionInfoDataResponse()
        {
            var sessionInfo = await baseFixture.LoginAdminAsync();

            var refreshedSessionInfo = await baseFixture.AccountApi.RefreshToken(sessionInfo.TokenResource);
            Assert.NotEqual(
                sessionInfo.TokenResource.AccessToken,
                refreshedSessionInfo.TokenResource.AccessToken);
        }

        [Fact]
        public async Task Post_RefreshToken_With_ExpiredToken_Returns_UnauthorizedResponse()
        {
            var config = new Dictionary<string, string>
            {
                {"BearerTokens:RefreshTokenExpirationSeconds", "10"}
            };
            baseFixture.AppState.CustomWebAppFactory = new CustomWebAppFactory<Startup>(config);
            //baseFixture.GenerateAppState(config);
            var sessionInfo = await baseFixture.LoginAdminAsync();

            // Wait until RefreshToken has expired  
            await Task.Delay(10000);

            await AssertHelper.AssertUnauthorizedAsync(
                () => baseFixture.AccountApi.RefreshToken(sessionInfo.TokenResource));
        }
    }
}