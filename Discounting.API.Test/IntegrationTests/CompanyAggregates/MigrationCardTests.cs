using System;
using System.Threading.Tasks;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.CompanyAggregates;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class MigrationCardTests : TestBase
    {
        private readonly MigrationCardFixture fixture;

        public MigrationCardTests(AppState appState) : base(appState)
        {
            fixture = new MigrationCardFixture(appState);
        }
        
        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.MigrationCardDataApi.Get(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.MigrationCardDataApi.Create(new Guid(), null));
            await AssertHelper.AssertUnauthorizedAsync(() =>fixture.MigrationCardDataApi.Update(new Guid(), new Guid(), null));
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
            var actual = await fixture.MigrationCardDataApi.Get(expected.CompanyId);
            Assert.Contains(actual, x => x.Id == expected.Id);
        }
        
        [Fact]
        public async Task Create_Then_Update_Updated()
        {
            await fixture.LoginSellerAsync();
            var dto = await fixture.CreateTestDataAsync();
            dto.RegistrationAddress = "updated data";
            var updatedDto = await fixture.MigrationCardDataApi.Update(dto.CompanyId, dto.Id, dto);
            Assert.NotNull(updatedDto);
            Assert.Equal(dto.Id, updatedDto.Id);
            Assert.Equal(dto.RegistrationAddress, updatedDto.RegistrationAddress);
        }
        
        [Fact]
        public async Task Create_Then_Create_Again_Same_Company_Other_Type_Created()
        {
            await fixture.LoginSellerAsync();
            var payload = fixture.GetPayload(GuidValues.CompanyGuids.TestSeller);
            payload.PositionType = CompanyPositionType.AuthorizedUser;
            var firstDto = await fixture.CreateTestDataAsync(payload);
            payload.PositionType = CompanyPositionType.Owner;
            var secondDto = await fixture.CreateTestDataAsync(payload);
            Assert.NotNull(secondDto);
            var infoDtos = await fixture.MigrationCardDataApi.Get(firstDto.CompanyId);
            Assert.Contains(infoDtos, x => x.Id == firstDto.Id);
            Assert.Contains(infoDtos, x => x.Id == secondDto.Id);
        }
    }
}