using System;
using System.Threading.Tasks;
using Discounting.Entities;
using Discounting.Entities.Regulations;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class RegulationTests : TestBase
    {
        public RegulationTests(AppState appState) : base(appState)
        {
            regulationFixture = new RegulationFixture(appState);
        }

        private readonly RegulationFixture regulationFixture;

        [Fact]
        public async Task Create_Logout_Get_Authorised()
        {
            await regulationFixture.LoginAdminAsync();
            var createdRegulation = await regulationFixture.CreateTestRegulationAsync();
            await regulationFixture.LogoutAsync();
            await regulationFixture.RegulationApi.Get(createdRegulation.Id);
            await regulationFixture.RegulationApi.Get();
        }

        [Fact]
        public async Task Create_Than_Change_Nothing_Update_Updated()
        {
            await regulationFixture.LoginAdminAsync();
            var createdRegulation = await regulationFixture.CreateTestRegulationAsync();
            var updatedRegulation = await regulationFixture.RegulationApi.Put(createdRegulation.Id, createdRegulation);
            Assert.NotNull(updatedRegulation);
            Assert.Equal(createdRegulation.Type, updatedRegulation.Type);
        }

        [Fact]
        public async Task Create_Than_Change_Type_Update_Updated()
        {
            await regulationFixture.LoginAdminAsync();
            var createdRegulation = await regulationFixture.CreateTestRegulationAsync();
            createdRegulation.Type = RegulationType.ETPPrivacyPolicy;
            var updatedRegulation = await regulationFixture.RegulationApi.Put(createdRegulation.Id, createdRegulation);
            Assert.NotNull(updatedRegulation);
            Assert.Equal(createdRegulation.Type, updatedRegulation.Type);
        }

        [Fact]
        public async Task Create_Than_Create_Again_With_Same_Type_Forbidden()
        {
            await regulationFixture.LoginAdminAsync();
            await regulationFixture.CreateTestRegulationAsync();
            await AssertHelper.AssertForbiddenAsync(() => regulationFixture.CreateTestRegulationAsync());
        }

        [Fact]
        public async Task Create_Than_Get_All_NotEmpty()
        {
            await regulationFixture.LoginAdminAsync();
            await regulationFixture.CreateTestRegulationAsync();
            var regulations = await regulationFixture.RegulationApi.Get();
            Assert.NotEmpty(regulations);
        }

        [Fact]
        public async Task Create_Than_Get_By_Id_NotNull()
        {
            await regulationFixture.LoginAdminAsync();
            var createdRegulation = await regulationFixture.CreateTestRegulationAsync();
            var regulationGetRequest = await regulationFixture.RegulationApi.Get(createdRegulation.Id);
            Assert.NotNull(regulationGetRequest);
            Assert.Equal(createdRegulation.Id, regulationGetRequest.Id);
        }

        [Fact]
        public async Task Create_Than_Get_By_Type_NotEmpty()
        {
            await regulationFixture.LoginAdminAsync();
            await regulationFixture.CreateTestRegulationAsync(
                regulationFixture.GetPayload(RegulationType.ETPPrivacyPolicy));
            var regulations = await regulationFixture.RegulationApi.Get(RegulationType.ETPPrivacyPolicy);
            Assert.NotEmpty(regulations);
            Assert.Single(regulations);
        }

        [Fact]
        public async Task Create_Two_Regulations_Than_Set_Same_Type_Update_Forbidden()
        {
            await regulationFixture.LoginAdminAsync();
            var createdRegulationOne = await regulationFixture.CreateTestRegulationAsync();
            var createdRegulationTwo =
                await regulationFixture.CreateTestRegulationAsync(
                    regulationFixture.GetPayload(RegulationType.ETPPrivacyPolicy));
            createdRegulationOne.Type = createdRegulationTwo.Type;
            await AssertHelper.AssertForbiddenAsync(() =>
                regulationFixture.RegulationApi.Put(createdRegulationOne.Id, createdRegulationOne));
        }

        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() => regulationFixture.RegulationApi.Post(null));
            await AssertHelper.AssertUnauthorizedAsync(() => regulationFixture.RegulationApi.Put(new Guid(), null));
        }
    }
}