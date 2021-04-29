using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Common.AccessControl;
using Discounting.Entities.Account;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;
using Xunit;
using static Discounting.Common.AccessControl.Operations;


namespace Discounting.Tests.IntegrationTests.AccessControl
{
    public class UserRoleTests : TestBase
    {
        public UserRoleTests(AppState appState) : base(appState)
        {
            baseFixture = new BaseFixture(appState);
        }

        private readonly BaseFixture baseFixture;

        [Fact]
        public async Task AssignNewRoleAndConsequentGet_Succeeds()
        {
            await baseFixture.LoginAdminAsync();

            var roleIds = new List<Guid>();

            // in the first part of this test, we are creating a few roles and
            // give each role a single permission. we then check if all roles
            // are correctly saved.

            for (var i = 0; i < 5; i++)
            {
                var role = await baseFixture.RoleApi.Create(new RoleDTO
                {
                    Name = "r" + i,
                    Permissions = new Dictionary<string, Operations>
                    {
                        {"access_control.users", Create}
                    },
                    Type = RoleType.InactiveCompany
                });

                roleIds.Add(role.Id);
            }

            var userRolesViaPost =
                await baseFixture.UserApi.SetRoles(GuidValues.UserGuids.TestSimpleUser, roleIds.ToArray());
            Assert.NotNull(userRolesViaPost);
            Assert.Equal(5, userRolesViaPost.Count);

            var rolesViaGet = await baseFixture.UserApi.GetRoles(GuidValues.UserGuids.TestSimpleUser);
            Assert.NotNull(rolesViaGet);

            // we are checking here whether all roles created previously
            // via POST have the correct number of permissions assigned
            // and also come with the correct label given during POST
            for (var i = 0; i < 5; i++)
                Assert.Contains(rolesViaGet, r =>
                    r.Name == "r" + i
                    && r.Permissions.Count == 1
                );

            // we now remove a permission and check if it is correctly removed and
            // the other permissions are still in the list after doing the update

            var roleIds2 = rolesViaGet.Where(r => r.Name != "r2").Select(r => r.Id).ToArray();
            var userRolesViaPost2 = await baseFixture.UserApi.SetRoles(GuidValues.UserGuids.TestSimpleUser, roleIds2);
            Assert.NotNull(userRolesViaPost2);
            Assert.Equal(4, userRolesViaPost2.Count);
        }
    }
}