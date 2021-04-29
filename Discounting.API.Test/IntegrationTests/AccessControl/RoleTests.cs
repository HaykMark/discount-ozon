using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Common.AccessControl;
using Discounting.Entities.Account;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Newtonsoft.Json;
using Xunit;
using static Discounting.Common.AccessControl.Operations;

namespace Discounting.Tests.IntegrationTests.AccessControl
{
    public class RoleTests : TestBase
    {
        public RoleTests(AppState appState) : base(appState)
        {
            roleFixture = new RoleFixture(appState);
        }

        private readonly RoleFixture roleFixture;

        [Fact]
        public async Task AssignNewRoleToUser_Succeeds()
        {
            await roleFixture.LoginAdminAsync();
            var (userIds, roleDto) = await roleFixture.CreateTestUsersAndRoles();

            var userRoles = await roleFixture.RoleApi.SetUserRoles(roleDto.Id, userIds);
            Assert.NotNull(userRoles);
            Assert.NotEmpty(userRoles);
            Assert.Equal(5, userRoles.Count);
            Assert.True(userRoles.All(u => u.RoleId == roleDto.Id));
            Assert.True(userRoles.All(u => userIds.Contains(u.UserId)));
        }

        [Fact]
        public async Task CreateNewRoleAndConsequentGetSucceeds()
        {
            await roleFixture.LoginAdminAsync();

            var payload = roleFixture.GetPayload();

            var responsePost = await roleFixture.RoleApi.Create(payload);

            Assert.NotNull(responsePost);
            Assert.True(responsePost.Id != default);
            Assert.Equal(payload.Name, responsePost.Name);
            Assert.Equal(payload.Description, responsePost.Description);
            Assert.Equal(payload.Remarks, responsePost.Remarks);
            Assert.Equal(payload.Permissions, responsePost.Permissions);

            var responseGetOne = await roleFixture.RoleApi.GetOne(responsePost.Id);

            Assert.Equal(
                JsonConvert.SerializeObject(responsePost),
                JsonConvert.SerializeObject(responseGetOne)
            );
        }

        [Fact]
        public async Task CreateNewRoleWithExistingNameFails()
        {
            await roleFixture.LoginAdminAsync();

            var dto = new RoleDTO
            {
                Name = "Test Role"
            };
            await roleFixture.RoleApi.Create(dto);

            await AssertHelper.FailOnStatusCodeAsync(() => roleFixture.RoleApi.Create(dto),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task DeleteRoleSucceeds()
        {
            await roleFixture.LoginAdminAsync();

            var responsePost = await roleFixture.RoleApi.Create(new RoleDTO
            {
                Name = "Test Role"
            });
            await roleFixture.RoleApi.Delete(responsePost.Id);
            await AssertHelper.FailOnStatusCodeAsync(() => roleFixture.RoleApi.GetOne(responsePost.Id),
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserRoles_Existing_NotEmpty()
        {
            await roleFixture.LoginAdminAsync();

            var (userIds, roleDto) = await roleFixture.CreateTestUsersAndRoles();
            await roleFixture.RoleApi.SetUserRoles(roleDto.Id, userIds);
            var userRoles = await roleFixture.RoleApi.GetUserRoles(roleDto.Id);
            Assert.NotEmpty(userRoles);
            Assert.True(userRoles.All(ur => ur.RoleId == roleDto.Id));
            Assert.Equal(userIds.Length, userRoles.Count);
        }

        [Fact]
        public async Task GetUsers_Existing_NotEmpty()
        {
            await roleFixture.LoginAdminAsync();

            var (userIds, roleDto) = await roleFixture.CreateTestUsersAndRoles();
            await roleFixture.RoleApi.SetUserRoles(roleDto.Id, userIds);
            var users = await roleFixture.RoleApi.GetUsers(roleDto.Id);
            Assert.NotEmpty(users);
            Assert.Equal(userIds.Length, users.Count);
            Assert.True(users.All(u => userIds.Contains(u.Id)));
        }

        // [Fact]
        // public async Task SeededSimpleUserHasOnlyReadRights()
        // {
        //     await roleFixture.LoginAdminAsync();
        //
        //     var role = await roleFixture.RoleApi.GetOne(GuidValues.RoleGuids.SimpleUser); // seeded roleFixture.RoleApi user
        //     Assert.Equal("Simple User", role.Name);
        //
        //     foreach (var operations in role.Permissions.Values) Assert.Equal(operations, Read);
        // }

        [Fact]
        public async Task UpdateExistingRoleSucceeds()
        {
            await roleFixture.LoginAdminAsync();

            var payload = roleFixture.GetPayload();

            var responsePost = await roleFixture.RoleApi.Create(payload);

            var rolePutDto = responsePost;
            responsePost.Description = "This is a test role with a changed description";
            responsePost.Name = "Test Role changed label";
            responsePost.Permissions = new Dictionary<string, Operations>
            {
                {"access_control.users", Create}
            };

            var responsePut = await roleFixture.RoleApi.Update(rolePutDto.Id, rolePutDto);

            Assert.NotNull(responsePut);
            Assert.Equal(responsePost.Id, responsePut.Id);

            Assert.NotEqual(payload.Name, rolePutDto.Name);
            Assert.Equal(responsePut.Name, rolePutDto.Name);

            Assert.NotEqual(payload.Description, rolePutDto.Description);
            Assert.Equal(responsePut.Description, rolePutDto.Description);

            Assert.NotEqual(payload.Permissions, rolePutDto.Permissions);
            Assert.Equal(responsePut.Permissions, rolePutDto.Permissions);
        }

        [Fact]
        public async Task UpdateMultipleRolesSucceeds()
        {
            await roleFixture.LoginAdminAsync();


            // -- first we create two roles --

            var role1 = roleFixture.GetPayload();
            role1.Name = "r1";
            role1.Permissions = new Dictionary<string, Operations>
            {
                {"access_control.users", Create}
            };
            role1 = await roleFixture.RoleApi.Create(role1);

            var role2 = roleFixture.GetPayload();
            role2.Permissions = new Dictionary<string, Operations>
            {
                {"access_control.users", Create}
            };
            role2.Name = "r2";
            role2 = await roleFixture.RoleApi.Create(role2);

            // -- now we change the permissions of role1 and PATCH --

            role1.Permissions = new Dictionary<string, Operations>
            {
                {"access_control.users", Update}
            };

            var updateRangeResponse = await roleFixture.RoleApi.UpdateRange(new List<RoleDTO> {role1});

            Assert.Equal(
                role1.Permissions,
                updateRangeResponse.First(r => r.Name == "r1").Permissions
            );

            // -- we also pull all roles and see if updates are persisted to be super sure --

            var allRolesResponse = await roleFixture.RoleApi.GetAll();

            Assert.Equal(
                allRolesResponse.First(r => r.Name == "r1").Permissions,
                role1.Permissions
            );

            Assert.Equal(
                allRolesResponse.First(r => r.Name == "r2").Permissions,
                role2.Permissions
            );
        }
    }
}