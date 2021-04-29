using System;
using System.Net;
using System.Threading.Tasks;
using Discounting.Entities;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class ContractTests : TestBase
    {
        public ContractTests(AppState appState) : base(appState)
        {
            contractFixture = new ContractFixture(appState);
        }

        private readonly ContractFixture contractFixture;

        [Fact]
        public async Task Get_Buyer_SellerBuyerContract_NotNull()
        {
            await contractFixture.LoginBuyerAsync();
            var actual = await contractFixture.ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            Assert.NotNull(actual);
            Assert.Equal(GuidValues.CompanyGuids.TestBuyer, actual.BuyerId);
        }

        [Fact]
        public async Task Get_SuperAdmin_AllContracts()
        {
            await contractFixture.LoginAdminAsync();
            var list = await contractFixture.ContractApi.Get();
            Assert.NotNull(list);
            Assert.NotEmpty(list);
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public async Task Get_SuperAdmin_GetBuyers_NotNull()
        {
            await contractFixture.LoginAdminAsync();
            var actual = await contractFixture.ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            Assert.NotNull(actual);
            Assert.Equal(GuidValues.CompanyGuids.TestBuyer, actual.BuyerId);
        }

        [Fact]
        public async Task GetAll_Buyer_SellerBuyerContract_NotEmpty()
        {
            await contractFixture.LoginBuyerAsync();
            var actual = await contractFixture.ContractApi.Get();
            Assert.NotEmpty(actual);
            Assert.Contains(actual, c => c.SellerId == GuidValues.CompanyGuids.TestBuyer ||
                                         c.BuyerId == GuidValues.CompanyGuids.TestSeller);
        }

        [Fact]
        public async Task GetAll_Seller_SellerBuyerContract_NotEmpty()
        {
            await contractFixture.LoginSellerAsync();
            var actual = await contractFixture.ContractApi.Get();
            Assert.NotEmpty(actual);
            Assert.Contains(actual, c => c.SellerId == GuidValues.CompanyGuids.TestBuyer ||
                                         c.BuyerId == GuidValues.CompanyGuids.TestSeller);
        }

        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() => contractFixture.ContractApi.Get(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() => contractFixture.ContractApi.Get());
            await AssertHelper.AssertUnauthorizedAsync(() => contractFixture.ContractApi.Post(null));
            await AssertHelper.AssertUnauthorizedAsync(() => contractFixture.ContractApi.Put(new Guid(), null));
        }

        [Fact]
        public async Task Post_ExitingConnection_ValidationException()
        {
            await contractFixture.LoginAdminAsync();
            await contractFixture.CreateContractAsync();
            await AssertHelper.FailOnStatusCodeAsync(() =>
                    contractFixture.CreateContractAsync(),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Post_NonExitingConnection_NoSellerClient_SellerCreated_ContractCreated()
        {
            await contractFixture.LoginAdminAsync();
            var payload = contractFixture.GetPayload();
            const string newTin = "1010101010";
            payload.SellerTin = newTin;

            var contractDto = await contractFixture.CreateContractAsync(payload);
            Assert.NotNull(contractDto);
            Assert.NotEqual(default, contractDto.Id);
            Assert.NotEqual(default, contractDto.SellerId);
            Assert.Equal(newTin, contractDto.SellerTin);
        }

        [Fact]
        public async Task Post_RightData_Ok()
        {
            await contractFixture.LoginAdminAsync();
            var contractDto = await contractFixture.CreateContractAsync();
            Assert.NotNull(contractDto);
            Assert.NotEqual(default, contractDto.Id);
        }

        [Fact]
        public async Task Put_ChangeSeller_ValidationException()
        {
            await contractFixture.LoginBuyerAsync();
            var contractDto = await contractFixture.ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            contractDto.SellerId = GuidValues.CompanyGuids.TestSimpleUser;
            contractDto.SellerId = GuidValues.CompanyGuids.TestSimpleUser;
            contractDto.SellerTin = TestConstants.TestSimpleUserTin;
            await AssertHelper.FailOnStatusCodeAsync(() =>
                    contractFixture.ContractApi.Put(contractDto.Id, contractDto),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Put_RightData_Updated()
        {
            await contractFixture.LoginBuyerAsync();
            var expected = await contractFixture.ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            expected.Status = ContractStatus.NotActive;
            expected.IsFactoring = true;
            await contractFixture.ContractApi.Put(expected.Id, expected);
            var actual = await contractFixture.ContractApi.Get(expected.Id);

            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.SellerId, actual.SellerId);
            Assert.Equal(expected.BuyerId, actual.BuyerId);
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.IsFactoring, actual.IsFactoring);
            Assert.Equal(expected.Provider, actual.Provider);
        }

        [Fact]
        public async Task Put_SameSellerBuyer_ValidationException()
        {
            await contractFixture.LoginBuyerAsync();
            var contractDto = await contractFixture.ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            contractDto.SellerId = GuidValues.CompanyGuids.TestBuyer;
            contractDto.SellerTin = TestConstants.TestBuyerTin;
            await AssertHelper.FailOnStatusCodeAsync(() =>
                    contractFixture.ContractApi.Put(contractDto.Id, contractDto),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task SimpleUser_GetSellerBuyerContract_NotFound()
        {
            await contractFixture.LoginSimpleUserAsync();
            await AssertHelper.FailOnStatusCodeAsync(() =>
                    contractFixture.ContractApi.Get(GuidValues.ContractGuids.TestBuyer),
                HttpStatusCode.NotFound);
        }
    }
}