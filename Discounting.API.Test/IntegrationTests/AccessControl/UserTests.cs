using System.Net;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.Account;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Newtonsoft.Json;
using Xunit;

namespace Discounting.Tests.IntegrationTests.AccessControl
{
    public class UserTests : TestBase
    {
        public UserTests(AppState appState) : base(appState)
        {
            userFixture = new UserFixture(appState);
        }

        private readonly UserFixture userFixture;

        [Fact]
        public async Task Create_User_Succeed()
        {
            await userFixture.LoginAdminAsync();
            var testUserDto = userFixture.GetPayload();
            var responsePost = await userFixture.UserApi.Create(testUserDto);

            Assert.NotNull(responsePost);
            Assert.True(responsePost.Id != default);
            Assert.Equal(testUserDto.Email, responsePost.Email);

            var responseGetOne = await userFixture.UserApi.GetOne(responsePost.Id);

            Assert.Equal(
                JsonConvert.SerializeObject(responsePost),
                JsonConvert.SerializeObject(responseGetOne)
            );
        }
        
        [Fact]
        public async Task SuperAdmin_Create_Then_Deactivate_Deactivated()
        {
            await userFixture.LoginAdminAsync();
            var payload = userFixture.GetPayload();
            payload.IsActive = true;
            var userDto = await userFixture.CreateTestUserAsync(payload);
            await userFixture.UserApi.Deactivate(userDto.Id, new DeactivationDTO
            {
                DeactivationReason = "test"
            });
            var deactivatedUser = await userFixture.UserApi.GetOne(userDto.Id);
            Assert.NotNull(deactivatedUser);
            Assert.False(string.IsNullOrEmpty(deactivatedUser.DeactivationReason));
            Assert.False(deactivatedUser.IsActive);
        }
        
        [Fact]
        public async Task SuperAdmin_Create_Then_Deactivate_Then_Activate_Activated()
        {
            await userFixture.LoginAdminAsync();
            var payload = userFixture.GetPayload();
            payload.IsActive = true;
            var userDto = await userFixture.CreateTestUserAsync(payload);
            await userFixture.UserApi.Deactivate(userDto.Id, new DeactivationDTO
            {
                DeactivationReason = "test"
            });
            
            await userFixture.UserApi.Activate(userDto.Id);
            var deactivatedUser = await userFixture.UserApi.GetOne(userDto.Id);
            Assert.NotNull(deactivatedUser);
            Assert.False(string.IsNullOrEmpty(deactivatedUser.DeactivationReason));
            Assert.True(deactivatedUser.IsActive);
        }

        // [Fact]
        // public async Task Create_User_Then_Confirm_Confirmed()
        // {
        //     await userFixture.LoginAdminAsync();
        //     var testUserDto = userFixture.GetPayload();
        //     var createdUserDto = await userFixture.UserApi.Create(testUserDto);
        //
        //     Assert.False(createdUserDto.IsConfirmedByAdmin);
        //
        //     createdUserDto.IsConfirmedByAdmin = true;
        //     
        //     var updateUserDto = await userFixture.UserApi.Update(createdUserDto.Id, createdUserDto);
        //
        //     Assert.NotNull(updateUserDto);
        //     Assert.True(updateUserDto.Id == createdUserDto.Id);
        //     Assert.Equal(testUserDto.Email, createdUserDto.Email);
        //     Assert.True(updateUserDto.IsConfirmedByAdmin);
        //
        //     var roles = await userFixture.UserApi.GetRoles(createdUserDto.Id);
        //     Assert.Contains(roles, r => r.Type == RoleType.SellerBuyer);
        // }

        [Fact]
        public async Task Delete_SuperAdminUser_Forbidden()
        {
            await userFixture.LoginAdminAsync();
            await AssertHelper.FailOnStatusCodeAsync(() =>
                userFixture.UserApi.Delete(GuidValues.UserGuids.Admin), HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Email_Address_Of_User_Should_Be_Unique()
        {
            await userFixture.LoginAdminAsync();
            var testUserDto = userFixture.GetPayload();

            var responsePost = await userFixture.UserApi.Create(testUserDto);
            await AssertHelper.FailOnStatusCodeAsync(() =>
                    userFixture.UserApi.Create(testUserDto),
                HttpStatusCode.Forbidden);
            responsePost.Email = "e.postavshik@yandex.ru";
            await AssertHelper.FailOnStatusCodeAsync(() =>
                    userFixture.UserApi.Update(responsePost.Id, responsePost),
                HttpStatusCode.Forbidden);

            const string newEmailAddress = "newtext@discounting.ru";
            responsePost.Email = newEmailAddress;
            responsePost = await userFixture.UserApi.Update(responsePost.Id, responsePost);

            Assert.Equal(responsePost.Email, newEmailAddress);
        }
    }
}