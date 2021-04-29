using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class FactoringAgreementTests : TestBase
    {
        public FactoringAgreementTests(AppState appState) : base(appState)
        {
            factoringAgreementFixture = new FactoringAgreementFixture(appState);
        }

        private readonly FactoringAgreementFixture factoringAgreementFixture;

        [Fact]
        public async Task Create_ForSimpleUser_Created()
        {
            var sessionInfoDto = await factoringAgreementFixture.LoginSimpleUserAsync();
            var payload = factoringAgreementFixture.GetPayload(sessionInfoDto.User.CompanyId);
            var favoriteDto = await factoringAgreementFixture
                .FactoringAgreementApi
                .Create(payload);
            Assert.NotNull(favoriteDto);
            Assert.NotNull(favoriteDto.SupplyFactoringAgreementDtos);
            Assert.NotEmpty(favoriteDto.SupplyFactoringAgreementDtos);
            Assert.False(favoriteDto.IsActive);
            Assert.False(favoriteDto.IsConfirmed);
            Assert.Equal(sessionInfoDto.User.CompanyId, favoriteDto.CompanyId);
            Assert.Equal(DateTime.UtcNow.Date, favoriteDto.CreationDate.Date);
            Assert.Equal(GuidValues.CompanyGuids.BankUserOne, favoriteDto.BankId);
        }

        [Fact]
        public async Task Create_ForSimpleUser_Without_SupplyAgreements_UnprocessableEntity()
        {
            var sessionInfoDto = await factoringAgreementFixture.LoginSimpleUserAsync();
            var payload = factoringAgreementFixture.GetPayload(sessionInfoDto.User.CompanyId);
            payload.SupplyFactoringAgreementDtos = new List<SupplyFactoringAgreementDTO>();
            var favoriteDto = await factoringAgreementFixture
                .FactoringAgreementApi
                .Create(payload);
            Assert.NotNull(favoriteDto);
            Assert.NotNull(favoriteDto.SupplyFactoringAgreementDtos);
            Assert.Empty(favoriteDto.SupplyFactoringAgreementDtos);
            Assert.False(favoriteDto.IsActive);
            Assert.False(favoriteDto.IsConfirmed);
            Assert.Equal(sessionInfoDto.User.CompanyId, favoriteDto.CompanyId);
            Assert.Equal(DateTime.UtcNow.Date, favoriteDto.CreationDate.Date);
            Assert.Equal(GuidValues.CompanyGuids.BankUserOne, favoriteDto.BankId);
        }

        [Fact]
        public async Task Create_Than_Confirm_Confirmed()
        {
            var sessionInfoDto = await factoringAgreementFixture.LoginSimpleUserAsync();
            var payload = factoringAgreementFixture.GetPayload(sessionInfoDto.User.CompanyId);
            var favoriteDto = await factoringAgreementFixture
                .FactoringAgreementApi
                .Create(payload);

            await factoringAgreementFixture.LoginBankAsync();

            favoriteDto.IsConfirmed = true;
            var updatedFavoriteDto = await factoringAgreementFixture
                .FactoringAgreementApi
                .Update(favoriteDto.Id, favoriteDto);

            Assert.NotNull(updatedFavoriteDto);
            Assert.NotNull(updatedFavoriteDto.SupplyFactoringAgreementDtos);
            Assert.NotEmpty(updatedFavoriteDto.SupplyFactoringAgreementDtos);
            Assert.True(updatedFavoriteDto.IsActive);
            Assert.True(updatedFavoriteDto.IsConfirmed);
            Assert.Equal(DateTime.UtcNow.Date, updatedFavoriteDto.CreationDate.Date);
            Assert.Equal(GuidValues.CompanyGuids.BankUserOne, updatedFavoriteDto.BankId);
        }

        [Fact]
        public async Task Create_Than_Confirm_Other_Bank_Forbidden()
        {
            var sessionInfoDto = await factoringAgreementFixture.LoginSimpleUserAsync();
            var payload = factoringAgreementFixture.GetPayload(sessionInfoDto.User.CompanyId);
            var favoriteDto = await factoringAgreementFixture
                .FactoringAgreementApi
                .Create(payload);

            await factoringAgreementFixture.LoginSecondBankAsync();

            favoriteDto.IsConfirmed = true;
            await AssertHelper.AssertForbiddenAsync(() => factoringAgreementFixture
                .FactoringAgreementApi
                .Update(favoriteDto.Id, favoriteDto));
        }

        [Fact]
        public async Task Get_All_NotEmpty()
        {
            var sessionInfoDto = await factoringAgreementFixture.LoginBuyerAsync();

            var favorites = await factoringAgreementFixture
                .FactoringAgreementApi
                .Get(sessionInfoDto.User.CompanyId);
            Assert.NotEmpty(favorites);
            Assert.True(favorites.All(f => f.CompanyId == sessionInfoDto.User.CompanyId));
        }

        [Fact]
        public async Task Get_All_FilteredBySupplyNumber_NotEmpty()
        {
            var sessionInfoDto = await factoringAgreementFixture.LoginBuyerAsync();

            var factoringAgreementDtos = await factoringAgreementFixture
                .FactoringAgreementApi
                .Get(supplyNumber: "first-supply-bank-contract-number-one");
            Assert.Single(factoringAgreementDtos);
            Assert.Equal(factoringAgreementDtos.Single().Id, GuidValues.FactoringAgreementGuid.TestBuyerBankOne);
        }

        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() =>
                factoringAgreementFixture.FactoringAgreementApi.Get(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() =>
                factoringAgreementFixture.FactoringAgreementApi.Create(null));
            await AssertHelper.AssertUnauthorizedAsync(() =>
                factoringAgreementFixture.FactoringAgreementApi.Update(new Guid(), null));
        }

        [Fact]
        public async Task Update_ForSimpleUser_Set_To_Active_Forbidden()
        {
            var sessionInfoDto = await factoringAgreementFixture.LoginSimpleUserAsync();
            var payload = factoringAgreementFixture.GetPayload(sessionInfoDto.User.CompanyId);
            var expected = await factoringAgreementFixture
                .FactoringAgreementApi
                .Create(payload);
            payload.BankId = GuidValues.CompanyGuids.BankUserSecond;

            var favoriteBankDto = await factoringAgreementFixture
                .FactoringAgreementApi
                .Create(payload);

            Assert.NotNull(favoriteBankDto);
            Assert.False(favoriteBankDto.IsActive);
            Assert.False(favoriteBankDto.IsConfirmed);
            Assert.Equal(sessionInfoDto.User.CompanyId, favoriteBankDto.CompanyId);
            Assert.Equal(DateTime.UtcNow.Date, favoriteBankDto.CreationDate.Date);
            Assert.Equal(GuidValues.CompanyGuids.BankUserSecond, favoriteBankDto.BankId);

            expected.IsActive = true;

            await AssertHelper.AssertForbiddenAsync(() => factoringAgreementFixture
                .FactoringAgreementApi
                .Update(expected.Id, expected));
        }
    }
}