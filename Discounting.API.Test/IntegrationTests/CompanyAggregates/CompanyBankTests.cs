using System;
using System.Threading.Tasks;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.CompanyAggregates;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class CompanyBankTests : TestBase
    {
        private readonly CompanyBankFixture fixture;

        public CompanyBankTests(AppState appState) : base(appState)
        {
            fixture = new CompanyBankFixture(appState);
        }

        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() => fixture.CompanyBankApi.Get(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() => fixture.CompanyBankApi.Create(new Guid(), null));
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
            var actual = await fixture.CompanyBankApi.Get(expected.CompanyId);
            Assert.Equal(expected.Id, actual.Id);
        }

        [Fact]
        public async Task Create_Then_Update_Success()
        {
            await fixture.LoginSellerAsync();
            var dto = await fixture.CreateTestDataAsync();
            dto.City = "updated city";
            var updatedDto = await fixture.CompanyBankApi.Create(dto.CompanyId, dto);
            Assert.NotNull(updatedDto);
            Assert.NotEqual(dto.Id, updatedDto.Id);
            Assert.Equal(dto.City, updatedDto.City);
        }

        [Fact]
        public async Task Create_Then_Create_Again_Same_Company_Id_Success()
        {
            await fixture.LoginSellerAsync();
            var dto = await fixture.CreateTestDataAsync();
            dto.Id = Guid.Empty;
            await fixture.CompanyBankApi.Create(dto.CompanyId, dto);
        }

        [Fact]
        public async Task Create_Bic_Not_Numeric_Fail()
        {
            await fixture.LoginSellerAsync();
            var payload = fixture.GetPayload(GuidValues.CompanyGuids.TestSeller);
            payload.Bic = "12345678q";
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                fixture.CompanyBankApi.Create(GuidValues.CompanyGuids.TestSeller, payload));
        }
    }
}