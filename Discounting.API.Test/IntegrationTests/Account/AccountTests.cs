using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discounting.Common.AccessControl;
using Discounting.Entities.CompanyAggregates;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Discounting.Tests.IntegrationTests.Account
{
    public class AccountTests : TestBase
    {
        public AccountTests(AppState appState) : base(appState)
        {
            accountFixture = new AccountFixture(appState);
        }

        private readonly AccountFixture accountFixture;

        [Fact]
        public async Task Get_GetUserInfo_Returns_OKResponse()
        {
            await accountFixture.LoginAdminAsync();

            // Assert for OK200 response
            // when requested with /account endpoint url.
            var userDto = await accountFixture.AccountApi.GetUserInfo();
            Assert.NotNull(userDto);
        }

        [Fact]
        public async Task Login_Admin_HasAdminRole()
        {
            var sessionInfo = await accountFixture.LoginAdminAsync();
            Assert.NotNull(sessionInfo.Roles);
            Assert.NotEmpty(sessionInfo.Roles);
            Assert.True(sessionInfo.Roles.All(r => r.Permissions.Any(p => p.Value == Operations.All)));
        }

        [Fact]
        public async Task Post_Login_Returns_SessionInfoDataResponse()
        {
            await accountFixture.LoginAdminAsync();
        }

        [Fact]
        public async Task Post_Logout_Success()
        {
            var sessionInfo = await accountFixture.LoginAdminAsync();

            await accountFixture.AccountApi.Logout(sessionInfo.TokenResource);
        }

        [Fact]
        public async Task Post_Super_Admin_Login_Returns_SessionInfoDataResponse()
        {
            await accountFixture.LoginSuperAdminAsync();
        }

        [Fact]
        public async Task Register_JoinCompany_Registered_CompanyNotCreated()
        {
            var payload = accountFixture.GetRegistrationPayload();
            await accountFixture.LoginAdminAsync();
            var createdCompany = await accountFixture.DbContext
                .Set<Company>()
                .AddAsync(new Company
                {
                    CompanyType = CompanyType.SellerBuyer,
                    TIN = payload.TIN
                });
            await accountFixture.DbContext.SaveChangesAsync();
            await accountFixture.LogoutAsync();
            var sessionInfo = await accountFixture.AccountApi.Register(payload);
            var company = await accountFixture.DbContext
                .Set<Company>()
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == sessionInfo.User.CompanyId);
            Assert.NotNull(sessionInfo);
            Assert.NotNull(company);
            Assert.NotNull(sessionInfo.User);
            Assert.NotNull(sessionInfo.User.Email);
            Assert.Equal(createdCompany.Entity.Id, sessionInfo.User.CompanyId);
            Assert.NotEqual(default, sessionInfo.User.CompanyId);
            Assert.Contains(company.Users, c => c.Id == sessionInfo.User.Id);
            Assert.False(sessionInfo.User.IsActive);
            Assert.Equal(payload.FullName, company.FullName);
            Assert.Equal(payload.ShortName, company.ShortName);
        }

        [Fact]
        public async Task UpdateOwnUserData_ViaUserEndpoint_Admin_Updated()
        {
            var session = await accountFixture.LoginAdminAsync();
            session.User.Surname = "test";
            await accountFixture.UserApi.Update(session.User.Id, session.User);
            var updatedUser = await accountFixture.UserApi.GetOne(session.User.Id);
            Assert.Equal(session.User.Surname, updatedUser.Surname);
        }
    }
}