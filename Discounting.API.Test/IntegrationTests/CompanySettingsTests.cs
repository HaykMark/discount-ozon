using System;
using System.Net;
using System.Threading.Tasks;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Fixtures.CompanyAggregates;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class CompanySettingsTests : TestBase
    {
        public CompanySettingsTests(AppState appState) : base(appState)
        {
            companyFixture = new CompanyFixture(appState);
        }

        private readonly CompanyFixture companyFixture;


        [Fact]
        public async Task Buyer_GetCompanySettings_Ok()
        {
            var sessionInfoDto = await companyFixture.LoginBuyerAsync();
            var settings = await companyFixture.CompanyApi.GetSettings(sessionInfoDto.User.CompanyId);
            Assert.NotNull(settings);
            Assert.Equal(sessionInfoDto.User.CompanyId, settings.CompanyId);
            Assert.Equal(sessionInfoDto.User.Id, settings.UserId);
        }

        [Fact]
        public async Task Buyer_UpdateSettings_Updated()
        {
            var sessionInfoDto = await companyFixture.LoginBuyerAsync();

            var expected = await companyFixture.CompanyApi.GetSettings(sessionInfoDto.User.CompanyId);
            expected.IsSendAutomatically = true;
            expected.IsAuction = false;
            await companyFixture.CompanyApi.UpdateSettings(sessionInfoDto.User.CompanyId, expected.Id, expected);
            var actual = await companyFixture.CompanyApi.GetSettings(sessionInfoDto.User.CompanyId);

            Assert.NotNull(actual);
            Assert.Equal(expected.CompanyId, actual.CompanyId);
            Assert.Equal(expected.UserId, actual.UserId);
            Assert.Equal(expected.IsSendAutomatically, actual.IsSendAutomatically);
            Assert.Equal(expected.IsAuction, actual.IsAuction);
        }


        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() => companyFixture.CompanyApi.GetSettings(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() =>
                companyFixture.CompanyApi.CreateSettings(new Guid(), null));
            await AssertHelper.AssertUnauthorizedAsync(() =>
                companyFixture.CompanyApi.UpdateSettings(new Guid(), new Guid(), null));
        }

        [Fact]
        public async Task SimpleUser_Create_DifferentCompany_UnprocessableEntity()
        {
            var sessionInfoDto = await companyFixture.LoginBuyerAsync();
            var payload = companyFixture.GetSettingsPayload(sessionInfoDto.User);
            payload.CompanyId = GuidValues.CompanyGuids.TestSeller;
            await AssertHelper.FailOnStatusCodeAsync(() =>
                    companyFixture.CompanyApi.CreateSettings(sessionInfoDto.User.CompanyId, payload),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task SimpleUser_Create_ExistingSettings_UnprocessableEntity()
        {
            var sessionInfoDto = await companyFixture.LoginBuyerAsync();
            var payload = companyFixture.GetSettingsPayload(sessionInfoDto.User);
            await AssertHelper.FailOnStatusCodeAsync(() =>
                    companyFixture.CompanyApi.CreateSettings(sessionInfoDto.User.CompanyId, payload),
                HttpStatusCode.UnprocessableEntity);
        }
    }
}