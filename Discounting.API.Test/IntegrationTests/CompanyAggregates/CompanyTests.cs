using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Fixtures.CompanyAggregates;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class CompanyTests : TestBase
    {
        public CompanyTests(AppState appState) : base(appState)
        {
            companyFixture = new CompanyFixture(appState);
        }

        private readonly CompanyFixture companyFixture;

        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() => companyFixture.CompanyApi.Get(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() => companyFixture.CompanyApi.Get());
            await AssertHelper.AssertUnauthorizedAsync(() => companyFixture.CompanyApi.GetUsers(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() => companyFixture.CompanyApi.Post(null));
            await AssertHelper.AssertUnauthorizedAsync(() => companyFixture.CompanyApi.Put(new Guid(), null));

            await AssertHelper.AssertUnauthorizedAsync(() => companyFixture.CompanyApi.GetSettings(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() =>
                companyFixture.CompanyApi.CreateSettings(new Guid(), null));
            await AssertHelper.AssertUnauthorizedAsync(() =>
                companyFixture.CompanyApi.UpdateSettings(new Guid(), new Guid(), null));
        }

        [Fact]
        public async Task SimpleUser_GetBuyer_Ok()
        {
            await companyFixture.LoginSimpleUserAsync();
            var buyer = await companyFixture.CompanyApi.Get(GuidValues.CompanyGuids.TestBuyer);
            Assert.NotNull(buyer);
            Assert.Equal(GuidValues.CompanyGuids.TestBuyer, buyer.Id);
        }

        [Fact]
        public async Task SimpleUser_GetByTin_NotEmpty()
        {
            await companyFixture.LoginSimpleUserAsync();
            var company = await companyFixture.CompanyApi.Get(tin: "0000000000");
            Assert.NotNull(company);
            Assert.Contains("0000000000", company.Select(c => c.TIN));
        }

        [Fact]
        public async Task SuperAdmin_Create_Ok()
        {
            await companyFixture.LoginAdminAsync();
            var companyDto = await companyFixture.CreateCompanyAsync();
            Assert.NotNull(companyDto);
            Assert.NotEqual(default, companyDto.Id);
        }

        [Fact]
        public async Task SuperAdmin_Get_All_NotEmpty()
        {
            await companyFixture.LoginAdminAsync();
            await companyFixture.CreateCompanyAsync();
            var list = await companyFixture.CompanyApi.Get();
            Assert.NotNull(list);
            Assert.NotEmpty(list);
        }

        [Fact]
        public async Task SuperAdmin_Get_AllUsers_NotEmpty()
        {
            var sessionDto = await companyFixture.LoginAdminAsync();
            var users = await companyFixture.CompanyApi.GetUsers(sessionDto.User.CompanyId);
            Assert.NotNull(users);
            Assert.NotEmpty(users);
            Assert.Contains(users, u => u.Id == sessionDto.User.Id);
        }

        [Fact]
        public async Task SuperAdmin_Get_One_NotNull()
        {
            var sessionDto = await companyFixture.LoginAdminAsync();
            var actual = await companyFixture.CompanyApi.Get(sessionDto.User.CompanyId);
            Assert.NotNull(actual);
            Assert.Equal(sessionDto.User.CompanyId, actual.Id);
        }

        [Fact]
        public async Task SuperAdmin_Update_Ok()
        {
            var sessionDto = await companyFixture.LoginAdminAsync();
            var expected = await companyFixture.CompanyApi.Get(sessionDto.User.CompanyId);
            expected.FullName = "new full name";
            expected.IsActive = !expected.IsActive;
            await companyFixture.CompanyApi.Put(expected.Id, expected);
            var actual = await companyFixture.CompanyApi.Get(sessionDto.User.CompanyId);

            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.FullName, actual.FullName);
            Assert.NotEqual(expected.IsActive, actual.IsActive);
        }
        
        [Fact]
        public async Task SuperAdmin_Create_Then_Deactivate_Deactivated()
        {
            await companyFixture.LoginAdminAsync();
            var companyDto = await companyFixture.CreateCompanyAsync();
            await companyFixture.CompanyApi.Deactivate(companyDto.Id, new DeactivationDTO
            {
                DeactivationReason = "test"
            });
            var deactivatedCompany = await companyFixture.CompanyApi.Get(companyDto.Id);
            Assert.NotNull(deactivatedCompany);
            Assert.False(string.IsNullOrEmpty(deactivatedCompany.DeactivationReason));
            Assert.False(deactivatedCompany.IsActive);
        }
        
        [Fact]
        public async Task SuperAdmin_Create_Then_Deactivate_Then_Activate_Activated()
        {
            await companyFixture.LoginAdminAsync();
            var companyDto = await companyFixture.CreateCompanyAsync();
            await companyFixture.CompanyApi.Deactivate(companyDto.Id, new DeactivationDTO
            {
                DeactivationReason = "test"
            });
            
            await companyFixture.CompanyApi.Activate(companyDto.Id);
            var deactivatedCompany = await companyFixture.CompanyApi.Get(companyDto.Id);
            Assert.NotNull(deactivatedCompany);
            Assert.False(string.IsNullOrEmpty(deactivatedCompany.DeactivationReason));
            Assert.True(deactivatedCompany.IsActive);
        }
    }
}