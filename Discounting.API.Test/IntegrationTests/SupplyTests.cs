using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels;
using Discounting.Entities;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class SupplyTests : TestBase
    {
        public SupplyTests(AppState appState) : base(appState)
        {
            supplyFixture = new SupplyFixture(appState);
        }

        private readonly SupplyFixture supplyFixture;

        [Fact]
        public async Task Get_Create_AsBuyer_InProcess_NotEmpty()
        {
            await supplyFixture.LoginBuyerAsync();
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            var supplies = await supplyFixture.SupplyApi.GetInProcess();
            Assert.NotEmpty(supplies);
            Assert.True(supplies.All(s => s.Status == SupplyStatus.InProcess));
            Assert.True(supplies.All(s => !string.IsNullOrEmpty(s.SellerTin)));
            Assert.True(supplies.All(s => !string.IsNullOrEmpty(s.BuyerTin)));
            Assert.True(supplies.All(s => s.BuyerTin == TestConstants.TestBuyerTin
                                          || s.SellerTin == TestConstants.TestBuyerTin));
            Assert.True(supplies.All(s => !s.UpdateDate.HasValue));
            Assert.True(supplies.All(s => !s.HasVerification));
            Assert.NotEmpty(supplies.Where(s =>
                supplyDto.Supplies.Select(ss => ss.Id).Contains(s.Id)));
            Assert.True(supplies.All(s => !string.IsNullOrWhiteSpace(s.ContractNumber)));
            Assert.True(supplies.All(s => s.ContractDate.Date == DateTime.UtcNow.Date));
        }

        [Fact]
        public async Task Get_Create_AsBuyer_NotAvailable_NotEmpty()
        {
            await supplyFixture.LoginBuyerAsync();
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            payload.ForEach(p => p.DelayEndDate = DateTime.UtcNow.AddDays(-1));
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            var supplies = await supplyFixture.SupplyApi.GetNotAvailable();
            Assert.NotEmpty(supplies);
            Assert.True(supplies.All(s => s.Status == SupplyStatus.NotAvailable));
            Assert.True(supplies.All(s => !string.IsNullOrEmpty(s.SellerTin)));
            Assert.True(supplies.All(s => !string.IsNullOrEmpty(s.BuyerTin)));
            Assert.True(supplies.All(s => s.BuyerTin == TestConstants.TestBuyerTin
                                          || s.SellerTin == TestConstants.TestBuyerTin));
            Assert.True(supplies.All(s => s.UpdateDate.HasValue));
            Assert.True(supplies.All(s => !s.HasVerification));
            Assert.NotEmpty(supplies.Where(s =>
                supplyDto.Supplies.Select(ss => ss.Id).Contains(s.Id)));
        }

        [Fact]
        public async Task Get_Create_WithSendAutomatically_AsSeller_NotInFinance()
        {
            //Prepare settings data
            var sessionDto = await supplyFixture.LoginSellerAsync();
            var settingsDto = await supplyFixture.CompanyApi.GetSettings(sessionDto.User.CompanyId);
            settingsDto.IsSendAutomatically = true;
            await supplyFixture.CompanyApi.UpdateSettings(sessionDto.User.CompanyId, settingsDto.Id, settingsDto);

            //Start test process
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            await supplyFixture.SupplyApi.Post(payload);

            var supplies = await supplyFixture.SupplyApi.GetInFinance();
            Assert.Empty(supplies);
        }

        [Fact]
        public async Task Get_InProgress_As_Admin_NotEmpty()
        {
            await supplyFixture.LoginAdminAsync();
            var supplies = await supplyFixture.SupplyApi.GetInProcess();
            Assert.NotEmpty(supplies);
        }

        [Fact]
        public async Task Post_AddChildSupplies_ThenMainSupplies_NoErrors_SuppliesConnected()
        {
            await supplyFixture.LoginBuyerAsync();
            var childSuppliesPayload = supplyFixture.ChildSuppliesPayload();
            var childSupplyResponseDto = await supplyFixture.SupplyApi.Post(childSuppliesPayload);

            Assert.NotNull(childSupplyResponseDto);
            Assert.NotEmpty(childSupplyResponseDto.Supplies);
            Assert.Equal(childSuppliesPayload.Count, childSupplyResponseDto.Supplies.Count);

            var mainSuppliesPayload = supplyFixture.MainSuppliesPayload();
            var mainSupplyResponseDto = await supplyFixture.SupplyApi.Post(mainSuppliesPayload);

            Assert.NotNull(mainSupplyResponseDto);
            Assert.NotEmpty(mainSupplyResponseDto.Supplies);
            Assert.Equal(mainSuppliesPayload.Count, mainSupplyResponseDto.Supplies.Count);

            foreach (var child in childSupplyResponseDto.Supplies)
            {
                var dto = await supplyFixture.SupplyApi.Get(child.Id);
                var main = mainSupplyResponseDto
                    .Supplies
                    .FirstOrDefault(m => m.Number == dto.BaseDocumentNumber);
                Assert.NotNull(main);
                Assert.Equal(main.Id, dto.BaseDocumentId);
            }
        }

        [Fact]
        public async Task Post_AddMainSupplies_ThenChildSupplies_NoErrors_SuppliesConnected()
        {
            await supplyFixture.LoginBuyerAsync();

            var mainSuppliesPayload = supplyFixture.MainSuppliesPayload();
            var mainSupplyResponseDto = await supplyFixture.SupplyApi.Post(mainSuppliesPayload);

            Assert.NotNull(mainSupplyResponseDto);
            Assert.NotEmpty(mainSupplyResponseDto.Supplies);
            Assert.Equal(mainSuppliesPayload.Count, mainSupplyResponseDto.Supplies.Count);

            var childSuppliesPayload = supplyFixture.ChildSuppliesPayload();
            var childSupplyResponseDto = await supplyFixture.SupplyApi.Post(childSuppliesPayload);

            Assert.NotNull(childSupplyResponseDto);
            Assert.NotEmpty(childSupplyResponseDto.Supplies);
            Assert.Equal(childSuppliesPayload.Count, childSupplyResponseDto.Supplies.Count);

            foreach (var child in childSupplyResponseDto.Supplies)
            {
                var dto = await supplyFixture.SupplyApi.Get(child.Id);
                var main = mainSupplyResponseDto
                    .Supplies
                    .FirstOrDefault(m => m.Number == dto.BaseDocumentNumber);
                Assert.NotNull(main);
                Assert.Equal(main.Id, dto.BaseDocumentId);
            }
        }

        [Fact]
        public async Task Post_AsBuyer_RightData_NoErrors_IsSendAutomatically_SuppliesCreated()
        {
            //Prepare settings data
            var sessionDto = await supplyFixture.LoginSellerAsync();
            var settingsDto = await supplyFixture.CompanyApi.GetSettings(sessionDto.User.CompanyId);
            settingsDto.IsSendAutomatically = true;
            await supplyFixture.CompanyApi.UpdateSettings(sessionDto.User.CompanyId, settingsDto.Id, settingsDto);
            await supplyFixture.LogoutAsync();

            //Start test process
            await supplyFixture.LoginBuyerAsync();
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Supplies);
            Assert.Empty(supplyDto.Errors);
            Assert.True(supplyDto.Supplies.All(s => s.Status == SupplyStatus.InFinance));
            Assert.Equal(payload.Count, supplyDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(supplyDto.Supplies));
        }

        [Fact]
        public async Task Post_AsBuyer_RightData_NoErrors_SuppliesCreated()
        {
            await supplyFixture.LoginBuyerAsync();
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Supplies);
            Assert.Empty(supplyDto.Errors);
            Assert.True(supplyDto.Supplies.All(s => s.Status == SupplyStatus.InProcess));
            Assert.Equal(payload.Count, supplyDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(supplyDto.Supplies));
            var supplies = await supplyFixture.SupplyApi.GetInProcess();
            Assert.NotEmpty(supplies);
        }

        [Fact]
        public async Task Post_AsBuyer_RightData_ThenVerify_AsSeller_Manually_SuppliesVerified_InFinance()
        {
            await supplyFixture.LoginBuyerAsync();

            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.True(supplyDto.Supplies.All(s => s.BuyerVerified));
            await supplyFixture.LogoutAsync();
            var sessionDto = await supplyFixture.LoginSellerAsync();
            var factoringAgreementDtos = await supplyFixture.FactoringAgreementApi.Get(sessionDto.User.CompanyId);
            var resultDto = await supplyFixture.SupplyApi.VerifySellerManually(new SupplyVerificationRequestDTO
            {
                BankId = factoringAgreementDtos.First().BankId,
                SupplyIds = supplyDto.Supplies.Select(s => s.Id).ToArray(),
                FactoringAgreementId = factoringAgreementDtos.First().Id
            });

            Assert.NotNull(resultDto);
            Assert.NotEmpty(resultDto.Supplies);
            Assert.Empty(resultDto.Errors);
            Assert.True(resultDto.Supplies.All(s => s.Status == SupplyStatus.InFinance));
            Assert.Equal(payload.Count, resultDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(resultDto.Supplies));
            var inFinanceSupplies = await supplyFixture.SupplyApi.GetInFinance();
            Assert.NotEmpty(inFinanceSupplies);
        }

        [Fact]
        public async Task Post_AsBuyer_WrongData_NoSupplies_ErrorsReturned()
        {
            await supplyFixture.LoginBuyerAsync();
            var payload = supplyFixture.GetWrongPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.Empty(supplyDto.Supplies);
            Assert.NotEmpty(supplyDto.Errors);
        }

        [Fact]
        public async Task Post_AsSeller_NoErrors_IsSendAutomatically_HasDefaultBank_SuppliesCreated()
        {
            //Prepare settings data
            var sessionDto = await supplyFixture.LoginSellerAsync();
            var settingsDto = await supplyFixture.CompanyApi.GetSettings(sessionDto.User.CompanyId);
            settingsDto.IsSendAutomatically = true;
            await supplyFixture.CompanyApi.UpdateSettings(sessionDto.User.CompanyId, settingsDto.Id, settingsDto);
            //Start test process
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Supplies);
            Assert.Empty(supplyDto.Errors);
            Assert.True(supplyDto.Supplies.All(s => s.Status == SupplyStatus.InProcess &&
                                                    !s.BankId.HasValue));
            Assert.Equal(payload.Count, supplyDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(supplyDto.Supplies));
            await supplyFixture.LogoutAsync();
            await supplyFixture.LoginSecondBankAsync();
            var supplies = await supplyFixture.SupplyApi.GetInProcess();
            Assert.Empty(supplies.Where(s =>
                supplyDto.Supplies.Select(ss => ss.Id).Contains(s.Id)));
        }

        [Fact]
        public async Task Post_AsSeller_RightData_NoContract_ValidationErrors()
        {
            await supplyFixture.LoginSellerAsync();
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            payload.ForEach(p => p.BuyerTin = TestConstants.TestAdminTin);
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Errors);
            Assert.Empty(supplyDto.Supplies);
            Assert.Equal(payload.Count, supplyDto.Errors.Count);
        }

        [Fact]
        public async Task Post_AsSeller_RightData_NoErrors_IsSendAutomatically_SuppliesCreated()
        {
            //Prepare settings data
            var sessionDto = await supplyFixture.LoginSellerAsync();
            var settingsDto = await supplyFixture.CompanyApi.GetSettings(sessionDto.User.CompanyId);
            settingsDto.IsSendAutomatically = true;
            await supplyFixture.CompanyApi.UpdateSettings(sessionDto.User.CompanyId, settingsDto.Id, settingsDto);

            //Start test process
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Supplies);
            Assert.Empty(supplyDto.Errors);
            Assert.True(supplyDto.Supplies.All(s => s.Status == SupplyStatus.InProcess));
            Assert.Equal(payload.Count, supplyDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(supplyDto.Supplies));
        }

        [Fact]
        public async Task Post_AsSeller_RightData_NoErrors_SuppliesCreated()
        {
            await supplyFixture.LoginSellerAsync();
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Supplies);
            Assert.Empty(supplyDto.Errors);
            Assert.True(supplyDto.Supplies.All(s => s.Status == SupplyStatus.InProcess));
            Assert.True(supplyDto.Supplies.All(s => s.SellerVerified == false));
            Assert.True(supplyDto.Supplies.All(s => s.BuyerVerified == false));
            Assert.True(supplyDto.Supplies.All(s => s.HasVerification == false));
            Assert.Equal(payload.Count, supplyDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(supplyDto.Supplies));
            var supplies = await supplyFixture.SupplyApi.GetInProcess();
            Assert.NotEmpty(supplies);
        }

        [Fact]
        public async Task Post_AsSeller_RightData_ThenVerify_AsSeller_Manually_SuppliesVerified_InProcess()
        {
            var sessionInfoDto = await supplyFixture.LoginSellerAsync();

            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.False(supplyDto.Supplies.All(s => s.BuyerVerified));

            await supplyFixture.LogoutAsync();
            await supplyFixture.LoginBuyerAsync();
            var inProcessBeforeVerificationBuyerSupplies = await supplyFixture.SupplyApi.GetInProcess();
            Assert.Empty(
                inProcessBeforeVerificationBuyerSupplies.Where(s => supplyDto.Supplies.Any(ss => ss.Id == s.Id)));
            await supplyFixture.LogoutAsync();
            await supplyFixture.LoginSellerAsync();

            var factoringAgreementDtos = await supplyFixture.FactoringAgreementApi.Get(sessionInfoDto.User.CompanyId);
            var resultDto = await supplyFixture.SupplyApi.VerifySellerManually(new SupplyVerificationRequestDTO
            {
                BankId = factoringAgreementDtos.First().BankId,
                SupplyIds = supplyDto.Supplies.Select(s => s.Id).ToArray(),
                FactoringAgreementId = factoringAgreementDtos.First().Id
            });

            Assert.NotNull(resultDto);
            Assert.NotEmpty(resultDto.Supplies);
            Assert.Empty(resultDto.Errors);
            Assert.True(resultDto.Supplies.All(s => s.Status == SupplyStatus.InProcess));
            Assert.True(resultDto.Supplies.All(s => s.SellerVerified));
            Assert.False(resultDto.Supplies.All(s => s.BuyerVerified));
            Assert.False(resultDto.Supplies.All(s => s.HasVerification));
            Assert.Equal(payload.Count, resultDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(resultDto.Supplies));
            var inProcessSupplies = await supplyFixture.SupplyApi.GetInProcess();
            Assert.NotEmpty(inProcessSupplies);
            await supplyFixture.LogoutAsync();
            await supplyFixture.LoginBuyerAsync();
            var inProcessAfterVerificationBuyerSupplies = await supplyFixture.SupplyApi.GetInProcess();
            Assert.NotEmpty(inProcessAfterVerificationBuyerSupplies);
        }

        [Fact]
        public async Task Post_AsSeller_RightData_ThenVerify_AsSeller_Than_AsBuyer_Manually_SuppliesVerified_InFinance()
        {
            var sessionInfoDto = await supplyFixture.LoginSellerAsync();

            var payload = supplyFixture.GetBuyerWithSellerPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);

            //Verify as seller
            var factoringAgreementDtos = await supplyFixture.FactoringAgreementApi.Get(sessionInfoDto.User.CompanyId);
            var resultDto = await supplyFixture.SupplyApi.VerifySellerManually(new SupplyVerificationRequestDTO
            {
                BankId = factoringAgreementDtos.First().BankId,
                SupplyIds = supplyDto.Supplies.Select(s => s.Id).ToArray(),
                FactoringAgreementId = factoringAgreementDtos.First().Id
            });

            //Verify as buyer

            await supplyFixture.LogoutAsync();
            await supplyFixture.LoginBuyerAsync();
            var buyerResultDto =
                await supplyFixture.SupplyApi.VerifyBuyerManually(resultDto.Supplies.Select(s => s.Id).ToArray());


            Assert.NotNull(buyerResultDto);
            Assert.NotEmpty(buyerResultDto.Supplies);
            Assert.Empty(buyerResultDto.Errors);
            Assert.True(buyerResultDto.Supplies.All(s => s.Status == SupplyStatus.InFinance));
            Assert.True(buyerResultDto.Supplies.All(s => s.SellerVerified));
            Assert.True(buyerResultDto.Supplies.All(s => s.BuyerVerified));
            Assert.True(buyerResultDto.Supplies.All(s => s.HasVerification));
            Assert.Equal(resultDto.Supplies.Count, buyerResultDto.Supplies.Count);
            var inProcessSupplies = await supplyFixture.SupplyApi.GetInProcess();
            var inFinanceSupplies = await supplyFixture.SupplyApi.GetInFinance();
            Assert.Empty(inProcessSupplies.Where(s => supplyDto.Supplies.Any(ss => ss.Id == s.Id)));
            Assert.NotEmpty(inFinanceSupplies.Where(s => supplyDto.Supplies.Any(ss => ss.Id == s.Id)));

            await supplyFixture.LogoutAsync();
            await supplyFixture.LoginSellerAsync();

            var inProcessAfterVerificationSellerSupplies = await supplyFixture.SupplyApi.GetInProcess();
            var inFinanceAfterVerificationSellerSupplies = await supplyFixture.SupplyApi.GetInFinance();
            Assert.True(inProcessAfterVerificationSellerSupplies.All(x =>
                !buyerResultDto.Supplies.Select(s => s.Id).Contains(x.Id)));
            Assert.True(inFinanceAfterVerificationSellerSupplies.All(x =>
                buyerResultDto.Supplies.Select(s => s.Id).Contains(x.Id)));
        }

        [Fact]
        public async Task Post_AsSeller_WrongData_NoSupplies_ErrorsReturned()
        {
            await supplyFixture.LoginSellerAsync();
            var payload = supplyFixture.GetWrongPayload();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.Empty(supplyDto.Supplies);
            Assert.NotEmpty(supplyDto.Errors);
        }

        [Fact]
        public async Task Post_RightAndWrongData_SuppliesCreated_ErrorsReturned()
        {
            await supplyFixture.LoginBuyerAsync();
            var payload = supplyFixture.GetWrongPayload()
                .Concat(supplyFixture.GetBuyerWithSellerPayload())
                .ToList();
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Supplies);
            Assert.NotEmpty(supplyDto.Errors);
            Assert.True(supplyFixture.ValidateResponse(supplyDto.Supplies));
        }

        [Fact]
        public async Task Post_RightData_NoContract_NoSeller_SuppliesCreated_ContractCreated_SellerCreated_NoErrors()
        {
            await supplyFixture.LoginBuyerAsync();
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            const string tin = "1212121212";
            payload.ForEach(p => p.SellerTin = tin);
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Supplies);
            Assert.Empty(supplyDto.Errors);
            Assert.Equal(payload.Count, supplyDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(supplyDto.Supplies));
            Assert.True(supplyDto.Supplies.All(s => s.ContractId != default));
            Assert.Single(supplyDto.Supplies.GroupBy(p => p.ContractId)
                .Select(g => g.First())
                .ToList());
            var contract = await supplyFixture.ContractApi.Get(supplyDto.Supplies.First().ContractId);
            Assert.NotNull(contract);
            Assert.Equal(tin, contract.SellerTin);
            Assert.NotEqual(default, contract.SellerId);
            var sellers = await supplyFixture.CompanyApi.Get(tin: tin);
            Assert.Single(sellers);
        }

        [Fact]
        public async Task Post_RightData_NoContract_SuppliesCreated_ContractCreated_NoErrors()
        {
            await supplyFixture.LoginBuyerAsync();
            var payload = supplyFixture.GetBuyerWithSellerPayload();
            payload.ForEach(p => p.SellerTin = TestConstants.TestAdminTin);
            var supplyDto = await supplyFixture.SupplyApi.Post(payload);
            Assert.NotNull(supplyDto);
            Assert.NotEmpty(supplyDto.Supplies);
            Assert.Empty(supplyDto.Errors);
            Assert.Equal(payload.Count, supplyDto.Supplies.Count);
            Assert.True(supplyFixture.ValidateResponse(supplyDto.Supplies));
            Assert.True(supplyDto.Supplies.All(s => s.ContractId != default));
            Assert.Single(supplyDto.Supplies.GroupBy(p => p.ContractId)
                .Select(g => g.First())
                .ToList());
            var contract = await supplyFixture.ContractApi.Get(supplyDto.Supplies.First().ContractId);
            Assert.NotNull(contract);
            Assert.Equal(TestConstants.TestAdminTin, contract.SellerTin);
        }
    }
}