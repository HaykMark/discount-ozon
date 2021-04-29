using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Entities.TariffDiscounting;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class DiscountSettingsTests : TestBase
    {
        private readonly DiscountSettingsFixture discountSettingsFixture;

        public DiscountSettingsTests(AppState appState) : base(appState)
        {
            discountSettingsFixture = new DiscountSettingsFixture(appState);
        }

        [Fact]
        public async Task Create_Created()
        {
            await discountSettingsFixture.LoginBuyerAsync();
            var discountSettingsDto = await discountSettingsFixture.CreateTestDiscountSettingsAsync();
            
            Assert.NotNull(discountSettingsFixture);
            Assert.Equal(DateTime.UtcNow.Date, discountSettingsDto.CreatedOn.Date);
            Assert.Null(discountSettingsDto.UpdatedOn);
        }
        
        [Fact]
        public async Task Create_Many_Then_Get_Current_Single()
        {
            //Preparations
            var sessionInfoDto = await discountSettingsFixture.LoginSellerAsync();
            var payload = discountSettingsFixture.GetPayload(sessionInfoDto.User.CompanyId);
            await discountSettingsFixture.CreateTestDiscountSettingsAsync(payload);
            
            //Act
            await discountSettingsFixture.LoginBuyerAsync();
            var expected = await discountSettingsFixture.CreateTestDiscountSettingsAsync();
            var actual = await discountSettingsFixture.DiscountSettingsApi.Get(true);
            Assert.Single(actual);
            Assert.Equal(expected.Id, actual.Single().Id);
        }

        [Fact]
        public async Task Create_Then_Update_Updated()
        {
            await discountSettingsFixture.LoginBuyerAsync();
            var discountSettingsDto = await discountSettingsFixture.CreateTestDiscountSettingsAsync();
            discountSettingsDto.PaymentWeekDays = DaysOfWeek.Monday | DaysOfWeek.Saturday;
            var updatedDiscountSettingsDto =
                await discountSettingsFixture.DiscountSettingsApi.Put(discountSettingsDto.Id, discountSettingsDto);
            
            Assert.NotNull(updatedDiscountSettingsDto);
            Assert.Equal(discountSettingsDto.Id, updatedDiscountSettingsDto.Id);
            Assert.Equal(discountSettingsDto.PaymentWeekDays, updatedDiscountSettingsDto.PaymentWeekDays);
            Assert.NotNull(updatedDiscountSettingsDto.UpdatedOn);
            Assert.Equal(DateTime.UtcNow.Date, updatedDiscountSettingsDto.UpdatedOn.Value.Date);
            Assert.Equal(discountSettingsDto.CreatedOn, updatedDiscountSettingsDto.CreatedOn);
        }
    }
}