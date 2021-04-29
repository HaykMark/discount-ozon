using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Common.AccessControl;
using Discounting.Entities.Account;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;
using static Discounting.Common.AccessControl.Operations;

namespace Discounting.Tests.Fixtures
{
    public class RoleFixture : BaseFixture
    {
        public RoleFixture(AppState appState = null) : base(appState)
        {
        }

        public async Task<(Guid[], RoleDTO)> CreateTestUsersAndRoles(RoleDTO payload = null)
        {
            var userIds = new Guid[5];
            var company = await CompanyFixture.CreateCompanyAsync();
            for (var i = 0; i < 5; i++)
            {
                var user = await UserApi.Create(new UserDTO
                {
                    Email = $"test{i}@email.de",
                    FirstName = "Test",
                    Surname = "Testyan",
                    SecondName = "Testi",
                    IsActive = true,
                    IsSuperAdmin = false,
                    CompanyId = company.Id
                });
                userIds[i] = user.Id;
            }

            payload ??= GetPayload();
            var roleDto = await RoleApi.Create(payload);
            return (userIds, roleDto);
        }

        public RoleDTO GetPayload()
        {
            return new RoleDTO
            {
                Name = "Test role",
                Description = "This is a test role",
                Remarks = "Some remarks",
                Type = RoleType.InactiveCompany,
                Permissions = new Dictionary<string, Operations>
                {
                    {
                        "access_control.users",
                        Create | Read
                    }
                }
            };
        }
    }
}