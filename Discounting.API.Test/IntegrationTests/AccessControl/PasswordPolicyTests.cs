using System.Threading.Tasks;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests.AccessControl
{
    public class PasswordPolicyTests : TestBase
    {
        public PasswordPolicyTests(AppState appState) : base(appState)
        {
            passwordPolicyFixture = new PasswordPolicyFixture(appState);
        }

        private readonly PasswordPolicyFixture passwordPolicyFixture;

        [Fact]
        public async Task ChangePassword_LowerUpperCaseNumberSpecialCharacters_Success()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("123456d@iscounT");

            // Act 
            var result = await passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Login_ExceedWrongPasswordLimit_AccountIsBlocked()
        {
            // Arrange
            for (var i = 0; i < 10; i++)
            {
                //Assert
                await AssertHelper.AssertForbiddenAsync(() =>
                    passwordPolicyFixture.LoginAsync("e.postavshik@yandex.ru", "1234!QAZwe"));
            }

            //Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var deactivatedUser = await passwordPolicyFixture.UserApi.GetOne(GuidValues.UserGuids.TestSeller);

            //Assert
            Assert.False(deactivatedUser.IsActive);
        }

        [Fact]
        public async Task Login_WrongPassword_Then_RightPassword_Authenticate_Success()
        {
            // Arrange
            for (var i = 0; i < 5; i++)
            {
                //Assert
                await AssertHelper.AssertForbiddenAsync(() =>
                    passwordPolicyFixture.LoginAsync("e.postavshik@yandex.ru", "1234!QAZwe"));
            }

            await passwordPolicyFixture.LoginSellerAsync();
            await passwordPolicyFixture.LogoutAsync();
            for (var i = 0; i < 9; i++)
            {
                //Assert
                await AssertHelper.AssertForbiddenAsync(() =>
                    passwordPolicyFixture.LoginAsync("e.postavshik@yandex.ru", "1234!QAZwe"));
            }

            await passwordPolicyFixture.LoginSellerAsync();
        }

        [Fact]
        public async Task ChangePassword_NotNumberOrSpecialCharacter_Failure()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("DISCOUNTtest");

            // Act / Assert
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto));
        }

        [Fact]
        public async Task ChangePassword_NotUpperCaseOrSpecialCharacter_Failure()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("123456discount");

            // Act / Assert
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto));
        }

        [Fact]
        public async Task ChangePassword_NotLowerCaseOrSpecialCharacter_Failure()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("123456DISCOUNT");

            // Act / Assert
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto));
        }

        [Fact]
        public async Task ChangePassword_InsufficientCharacterLenght_Failure()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("123Discount");

            // Act / Assert
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto));
        }

        [Fact]
        public async Task ChangePassword_LowerUpperCaseNumber_Fail()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("123456Discount");

            // Act 
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto));
        }

        [Fact]
        public async Task ChangePassword_LowerUpperCaseSpecialCharacters_Fail()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("DISCoP@SSWORD");

            // Act 
            // Act 
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto));
        }

        [Fact]
        public async Task ChangePassword_UpperCaseNumberSpecialCharacters_Fail()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("123456D@ISCO");

            // Act 
            // Act 
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto));
        }

        [Fact]
        public async Task ChangePassword_SamePassword_Fail()
        {
            // Arrange
            await passwordPolicyFixture.LoginAdminAsync();
            var userDto = await passwordPolicyFixture.SetupTestPasswordPolicyAsync();
            var passwordDto = passwordPolicyFixture.GetPayload("123456D!iscount");
            await passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto);

            // Act 
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                passwordPolicyFixture.UserApi.ChangePassword(userDto.Id, passwordDto));
        }
    }
}