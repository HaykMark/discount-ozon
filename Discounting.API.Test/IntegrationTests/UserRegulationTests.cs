using System.Threading.Tasks;
using Discounting.Entities.Regulations;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class UserRegulationTests : TestBase
    {
        public UserRegulationTests(AppState appState) : base(appState)
        {
            regulationFixture = new UserRegulationFixture(appState);
        }

        private readonly UserRegulationFixture regulationFixture;

        [Fact]
        public async Task Create_Profile_Created()
        {
            await regulationFixture.LoginSimpleUserAsync();

            var createdRegulation = await regulationFixture.CreateTestUserRegulationAsync();
            Assert.NotNull(createdRegulation);
            Assert.NotNull(createdRegulation.UserProfileRegulationInfo);
            Assert.Equal(createdRegulation.Id, createdRegulation.UserProfileRegulationInfo.UserRegulationId);
            Assert.Equal(UserRegulationType.Profile, createdRegulation.Type);
        }
        
        [Fact]
        public async Task Create_Other_Created()
        {
            await regulationFixture.LoginSimpleUserAsync();
            
            var user = await regulationFixture.UserFixture.CreateTestUserAsync();
            var payload = regulationFixture.GetPayload(user.Id, UserRegulationType.Other);
            var createdRegulation = await regulationFixture.CreateTestUserRegulationAsync(payload);
            Assert.NotNull(createdRegulation);
            Assert.Null(createdRegulation.UserProfileRegulationInfo);
            Assert.Equal(UserRegulationType.Other, createdRegulation.Type);
        }
        
        [Fact]
        public async Task Create_Update_Profile_Updated()
        {
            await regulationFixture.LoginSimpleUserAsync();
            
            var createdRegulation = await regulationFixture.CreateTestUserRegulationAsync();
            createdRegulation.UserProfileRegulationInfo.Number = "test2";
            var updatedUserProfileRegulationInfo = await regulationFixture.UserRegulationApi.Put(createdRegulation.Id,
                createdRegulation.UserProfileRegulationInfo);
            Assert.NotNull(updatedUserProfileRegulationInfo);
            Assert.Equal(createdRegulation.UserProfileRegulationInfo.Number, updatedUserProfileRegulationInfo.Number);
        }
    }
}