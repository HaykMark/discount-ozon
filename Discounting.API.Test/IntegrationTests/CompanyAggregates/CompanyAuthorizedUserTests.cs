using System;
using System.Threading.Tasks;
using Discounting.Tests.Fixtures.CompanyAggregates;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class CompanyAuthorizedUserTests : TestBase
    {
        public CompanyAuthorizedUserTests(AppState appState) : base(appState)
        {
            fixture = new CompanyAuthorizedUserFixture(appState);
        }

        private readonly CompanyAuthorizedUserFixture fixture;

        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.CompanyAuthorizedUserApi.Get(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.CompanyAuthorizedUserApi.Create(new Guid(), null));
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.CompanyAuthorizedUserApi.Update(new Guid(), new Guid(), null));
        }

        [Fact]
        public async Task Create_Created()
        {
            await fixture.LoginSellerAsync();
            var dto = await fixture.CreateTestDataAsync();
            Assert.NotNull(dto);
        }

        [Fact]
        public async Task Create_Then_Get_NotNull()
        {
            await fixture.LoginSellerAsync();
            var expected = await fixture.CreateTestDataAsync();
            var actual = await fixture.CompanyAuthorizedUserApi.Get(expected.CompanyId);
            Assert.Equal(expected.Id, actual.Id);
        }
        
        [Fact]
        public async Task Create_Then_Update_Updated()
        {
            await fixture.LoginSellerAsync();
            var dto = await fixture.CreateTestDataAsync();
            dto.Number = "updated data";
            var updatedDto = await fixture.CompanyAuthorizedUserApi.Update(dto.CompanyId, dto.Id, dto);
            Assert.NotNull(updatedDto);
            Assert.Equal(dto.Id, updatedDto.Id);
            Assert.Equal(dto.Number, updatedDto.Number);
        }
        
        [Fact]
        public async Task Create_Then_Create_Again_Same_Company_Id_Forbidden()
        {
            await fixture.LoginSellerAsync();
            var dto = await fixture.CreateTestDataAsync();
            dto.Id = Guid.Empty;
            await AssertHelper.AssertForbiddenAsync(() => fixture.CompanyAuthorizedUserApi.Create(dto.CompanyId, dto));
        }
    }
}