using System.Linq;
using System.Threading.Tasks;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class BuyerTemplateConnectionTests : TestBase
    {
        public BuyerTemplateConnectionTests(AppState appState) : base(appState)
        {
            buyerTemplateConnectionFixture = new BuyerTemplateConnectionFixture(appState);
        }

        private readonly BuyerTemplateConnectionFixture buyerTemplateConnectionFixture;

        [Fact]
        public async Task Create_Created()
        {
            await buyerTemplateConnectionFixture.LoginSecondBankAsync();
            
            var expected = buyerTemplateConnectionFixture.GetPayload();
            var actual = await buyerTemplateConnectionFixture.CreateTestConnectionAsync(expected);
            
            Assert.NotNull(expected);
            Assert.Equal(expected.TemplateId, actual.TemplateId);
            Assert.Equal(expected.BuyerId, actual.BuyerId);
        }

        [Fact]
        public async Task Create_Than_GetAll_Contains()
        {
            await buyerTemplateConnectionFixture.LoginSecondBankAsync();
            
            var expected = await buyerTemplateConnectionFixture.CreateTestConnectionAsync();
            var actual = await buyerTemplateConnectionFixture.BuyerTemplateConnectionApi.Get();

            Assert.Contains(actual, x => x.Id == expected.Id);
        }

        [Fact]
        public async Task Create_Than_GetOne_Equal()
        {
            await buyerTemplateConnectionFixture.LoginSecondBankAsync();
            
            var expected = await buyerTemplateConnectionFixture.CreateTestConnectionAsync();
            var actual = await buyerTemplateConnectionFixture.BuyerTemplateConnectionApi.Get(expected.Id);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.TemplateId, actual.TemplateId);
            Assert.Equal(expected.BuyerId, actual.BuyerId);
        }

        [Fact]
        public async Task Create_Than_Create_Again_Same_Connections_Forbidden()
        {
            await buyerTemplateConnectionFixture.LoginSecondBankAsync();
            
            var payload = buyerTemplateConnectionFixture.GetPayload();
            await buyerTemplateConnectionFixture.CreateTestConnectionAsync(payload);

            await AssertHelper.AssertForbiddenAsync(() =>
                buyerTemplateConnectionFixture.BuyerTemplateConnectionApi.Post(payload));
        }
        
        [Fact]
        public async Task Create_Than_Update_Template_Updated()
        {
            await buyerTemplateConnectionFixture.LoginSecondBankAsync();
            
            await buyerTemplateConnectionFixture.BuyerTemplateConnectionApi.Delete(GuidValues
                .BuyerTemplateConnectionGuids.TestBuyerVerification);
            var payload = buyerTemplateConnectionFixture.GetPayload();
            var createdDto = await buyerTemplateConnectionFixture.CreateTestConnectionAsync(payload);
            createdDto.TemplateId = GuidValues.TemplateGuids.Verification;
            var updatedDto = await buyerTemplateConnectionFixture.BuyerTemplateConnectionApi.Put(createdDto.Id, createdDto);
            
            Assert.NotNull(updatedDto);
            Assert.Equal(createdDto.Id, updatedDto.Id);
            Assert.Equal(createdDto.TemplateId, updatedDto.TemplateId);
            Assert.Equal(createdDto.BuyerId, updatedDto.BuyerId);
        }
        
        [Fact]
        public async Task Create_Than_Update_Buyer_Updated()
        {
            await buyerTemplateConnectionFixture.LoginSecondBankAsync();
            
            var payload = buyerTemplateConnectionFixture.GetPayload();
            var createdDto = await buyerTemplateConnectionFixture.CreateTestConnectionAsync(payload);
            createdDto.BuyerId = GuidValues.CompanyGuids.TestSeller;

            var updatedDto = await buyerTemplateConnectionFixture.BuyerTemplateConnectionApi.Put(createdDto.Id, createdDto);
            
            Assert.NotNull(updatedDto);
            Assert.Equal(createdDto.Id, updatedDto.Id);
            Assert.Equal(createdDto.TemplateId, updatedDto.TemplateId);
            Assert.Equal(createdDto.BuyerId, updatedDto.BuyerId);
        }
    }
}