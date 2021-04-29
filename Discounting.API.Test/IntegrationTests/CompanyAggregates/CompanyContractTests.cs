using System;
using System.Threading.Tasks;
using Discounting.Tests.Fixtures.CompanyAggregates;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class CompanyContractTests : TestBase
    {
        private readonly CompanyContactFixture fixture;

        public CompanyContractTests(AppState appState) : base(appState)
        {
            fixture = new CompanyContactFixture(appState);
        }
        
        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.CompanyContactApi.Get(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.CompanyContactApi.Create(new Guid(), null));
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.CompanyContactApi.Update(new Guid(), new Guid(), null));
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
            var actual = await fixture.CompanyContactApi.Get(expected.CompanyId);
            Assert.Equal(expected.Id, actual.Id);
        }
        
        [Fact]
        public async Task Create_Then_Update_Updated()
        {
            await fixture.LoginSellerAsync();
            var dto = await fixture.CreateTestDataAsync();
            dto.Address = "updated data";
            var updatedDto = await fixture.CompanyContactApi.Update(dto.CompanyId, dto.Id, dto);
            Assert.NotNull(updatedDto);
            Assert.Equal(dto.Id, updatedDto.Id);
            Assert.Equal(dto.Address, updatedDto.Address);
        }
        
        [Fact]
        public async Task Create_Then_Create_Again_Same_Company_Id_Forbidden()
        {
            await fixture.LoginSellerAsync();
            var dto = await fixture.CreateTestDataAsync();
            dto.Id = Guid.Empty;
            await AssertHelper.AssertForbiddenAsync(() => fixture.CompanyContactApi.Create(dto.CompanyId, dto));
        }
    }
}