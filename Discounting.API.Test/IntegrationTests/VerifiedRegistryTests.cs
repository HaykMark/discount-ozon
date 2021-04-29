using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Entities;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class VerifiedRegistryTests : TestBase
    {
        public VerifiedRegistryTests(AppState appState) : base(appState)
        {
            registryFixture = new RegistryFixture(appState);
        }

        private readonly RegistryFixture registryFixture;


        [Fact]
        public async Task Create_Created()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            Assert.NotNull(registry);
            Assert.Equal(RegistryStatus.InProcess, registry.Status);
            Assert.Equal(RegistrySignStatus.NotSigned, registry.SignStatus);
            Assert.Equal(DateTime.UtcNow.Date, registry.Date.Date);

            var supplies = await registryFixture.RegistryApi.GetSupplies(registry.Id);
            Assert.NotEmpty(supplies);
            Assert.True(supplies.All(s => s.RegistryId == registry.Id));

            await registryFixture.LogoutAsync();
            await registryFixture.LoginSecondBankAsync();
            var contract = await registryFixture.ContractApi.Get(registry.ContractId);
            Assert.NotNull(contract);
            Assert.Equal(registry.ContractId, contract.Id);

            await registryFixture.RegistryApi.Put(registry.Id, registry);
            await registryFixture.LogoutAsync();
            await registryFixture.LoginSecondBankAsync();
            var financedSupplies = await registryFixture.SupplyApi.GetInFinance();
            Assert.DoesNotContain(financedSupplies, s => s.RegistryId == registry.Id);
        }

        [Fact]
        public async Task Create_DeleteSomeSupplies_Deleted()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            var financedSupplies = await registryFixture.SupplyApi.GetInFinance();
            var lastSupply = financedSupplies.Last();
            financedSupplies.Remove(lastSupply);
            var updatedRegistry =
                await registryFixture.RegistryApi.SetSupplies(registry.Id,
                    financedSupplies.Select(s => s.Id).ToArray());
            Assert.NotNull(updatedRegistry);
            Assert.Equal(registry.Id, updatedRegistry.Id);
            var updatedFinancedSupplies = await registryFixture.SupplyApi.GetInFinance();
            Assert.True(financedSupplies.All(s => s.Id != lastSupply.Id));

            var updatedLastSupply = await registryFixture.SupplyApi.Get(lastSupply.Id);
            Assert.True(updatedLastSupply.Status == SupplyStatus.InProcess);
            Assert.Null(updatedLastSupply.RegistryId);
        }

        [Fact]
        public async Task Create_GetAll_Bank_Empty()
        {
            await registryFixture.CreateVerificationTestRegistryAsync();
            await registryFixture.LogoutAsync();
            await registryFixture.LoginSecondBankAsync();
            var inProcessRegistriesBank = await registryFixture.RegistryApi.GetInProcess();
            Assert.Empty(inProcessRegistriesBank);
        }

        [Fact]
        public async Task Create_GetAll_Buyer_Empty()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            await registryFixture.LogoutAsync();
            await registryFixture.LoginBuyerAsync();
            var inProcessRegistriesBuyer = await registryFixture.RegistryApi.GetInProcess();
            Assert.Empty(inProcessRegistriesBuyer);
        }

        [Fact]
        public async Task Create_GetAll_Seller_NotEmpty()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            var inProcessRegistriesSeller = await registryFixture.RegistryApi.GetInProcess();
            Assert.NotEmpty(inProcessRegistriesSeller);
            Assert.NotEmpty(inProcessRegistriesSeller.Where(r => r.Id == registry.Id));
            var financedSupplies = await registryFixture.SupplyApi.GetInFinance();
            Assert.NotEmpty(financedSupplies);
            Assert.True(financedSupplies.All(s => s.RegistryId == registry.Id));
        }

        [Fact]
        public async Task Create_Seller_Decline_Declined_GetDeclined_NotEmpty()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            registry.Status = RegistryStatus.Declined;
            var declinedRegistry = await registryFixture.RegistryApi.Put(registry.Id, registry);
            Assert.NotNull(declinedRegistry);
            Assert.Equal(RegistryStatus.Declined, declinedRegistry.Status);
            var declinedRegistries = await registryFixture.RegistryApi.GetDeclined();
            Assert.NotEmpty(declinedRegistries);
            Assert.NotEmpty(declinedRegistries.Where(r => r.Id == registry.Id));

            var inProcessSupplies = await registryFixture.SupplyApi.GetInProcess();
            Assert.NotEmpty(inProcessSupplies);
            Assert.True(inProcessSupplies.All(s => s.RegistryId == null));

            var registrySupplies = await registryFixture.RegistryApi.GetSupplies(registry.Id);
            Assert.Empty(registrySupplies);
        }

        [Fact]
        public async Task Create_SignBySeller_DeclineBySeller_Forbidden()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            registry.SignStatus = RegistrySignStatus.SignedBySeller;
            await registryFixture.RegistryApi.Put(registry.Id, registry);
            registry.Status = RegistryStatus.Declined;
            await AssertHelper.AssertForbiddenAsync(() => registryFixture.RegistryApi.Put(registry.Id, registry));
        }

        [Fact]
        public async Task Create_SignBySeller_DeclineBySeller_LoginBuyer_InProcess_Empty()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            registry.Status = RegistryStatus.Declined;
            await registryFixture.RegistryApi.Put(registry.Id, registry);
            await registryFixture.LogoutAsync();
            await registryFixture.LoginBuyerAsync();
            var buyerRegistries = await registryFixture.RegistryApi.GetInProcess();
            Assert.Empty(buyerRegistries);
        }

        [Fact]
        public async Task Create_SignBySeller_GetAll_Buyer_NotEmpty()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            registry.SignStatus = RegistrySignStatus.SignedBySeller;
            await registryFixture.RegistryApi.Put(registry.Id, registry);
            await registryFixture.LogoutAsync();
            await registryFixture.LoginBuyerAsync();
            var inProcessRegistriesBuyer = await registryFixture.RegistryApi.GetInProcess();
            Assert.NotEmpty(inProcessRegistriesBuyer);
            Assert.NotEmpty(inProcessRegistriesBuyer.Where(r => r.Id == registry.Id));
        }

        [Fact]
        public async Task Create_SignBySeller_LoginBuyer_InProcess_RightData()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            registry.SignStatus = RegistrySignStatus.SignedBySeller;
            await registryFixture.RegistryApi.Put(registry.Id, registry);
            await registryFixture.LogoutAsync();
            await registryFixture.LoginBuyerAsync();
            var buyerRegistries = await registryFixture.RegistryApi.GetInProcess();
            Assert.NotEmpty(buyerRegistries);
            Assert.True(buyerRegistries.All(r => r.Status == RegistryStatus.InProcess));
            var buyerFinished = await registryFixture.RegistryApi.GetFinished();
            Assert.Empty(buyerFinished);
        }

        [Fact]
        public async Task Create_UpdateToFinished_GetFinished_Bank_NotEmpty()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();

            await registryFixture.UpdateRegistryStatusAsync(registry.Id, RegistryStatus.Finished);
            
            var inProcessRegistriesBank = await registryFixture.RegistryApi.GetFinished();
            Assert.NotEmpty(inProcessRegistriesBank);
            Assert.NotEmpty(inProcessRegistriesBank.Where(r => r.Id == registry.Id));
            var registrySupplies = await registryFixture.RegistryApi.GetSupplies(registry.Id);

            Assert.NotEmpty(registrySupplies);
            Assert.True(registrySupplies.All(s => s.Status == SupplyStatus.InFinance));
            var financedSupplies = await registryFixture.SupplyApi.GetInFinance();
            Assert.NotEmpty(financedSupplies);
            Assert.Contains(financedSupplies, s => s.RegistryId == registry.Id);
        }

        [Fact]
        public async Task Create_UpdateToFinished_GetFinished_Buyer_NotEmpty()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            await registryFixture.UpdateRegistryStatusAsync(registry.Id, RegistryStatus.Finished);
            
            var inProcessRegistriesBuyer = await registryFixture.RegistryApi.GetFinished();
            Assert.NotEmpty(inProcessRegistriesBuyer);
            Assert.NotEmpty(inProcessRegistriesBuyer.Where(r => r.Id == registry.Id));
            var registrySupplies = await registryFixture.RegistryApi.GetSupplies(registry.Id);
            Assert.NotEmpty(registrySupplies);
            Assert.True(registrySupplies.All(s => s.Status == SupplyStatus.InFinance));
            var financedSupplies = await registryFixture.SupplyApi.GetInFinance();
            Assert.NotEmpty(financedSupplies);
            Assert.Contains(financedSupplies, s => s.RegistryId == registry.Id);
        }

        [Fact]
        public async Task Create_UpdateToFinished_GetFinished_Seller_NotEmpty()
        {
            var registry = await registryFixture.CreateVerificationTestRegistryAsync();
            await registryFixture.UpdateRegistryStatusAsync(registry.Id, RegistryStatus.Finished);
            var inProcessRegistriesSeller = await registryFixture.RegistryApi.GetFinished();
            Assert.NotEmpty(inProcessRegistriesSeller);
            Assert.NotEmpty(inProcessRegistriesSeller.Where(r => r.Id == registry.Id));
            var registrySupplies = await registryFixture.RegistryApi.GetSupplies(registry.Id);
            Assert.NotEmpty(registrySupplies);
            Assert.True(registrySupplies.All(s => s.Status == SupplyStatus.InFinance));
            var financedSupplies = await registryFixture.SupplyApi.GetInFinance();
            Assert.NotEmpty(financedSupplies);
            Assert.Contains(financedSupplies, s => s.RegistryId == registry.Id);
        }

        [Fact]
        public async Task Get_InProgress_As_Admin_NotEmpty()
        {
            await registryFixture.CreateVerificationTestRegistryAsync();
            await registryFixture.LoginAdminAsync();
            var inProcessRegistriesSeller = await registryFixture.RegistryApi.GetInProcess();
            Assert.NotEmpty(inProcessRegistriesSeller);
        }

        [Fact]
        public async Task Update_Number_Updated()
        {
            var expected = await registryFixture.CreateVerificationTestRegistryAsync();
            var actual = await registryFixture.RegistryApi.Put(expected.Id, expected);

            Assert.NotNull(actual);
            Assert.Equal(expected.Status, expected.Status);
            Assert.Equal(expected.SignStatus, actual.SignStatus);
            Assert.Equal(expected.Number, actual.Number);


            var supplies = await registryFixture.RegistryApi.GetSupplies(actual.Id);
            Assert.NotEmpty(supplies);
            Assert.True(supplies.All(s => s.RegistryId == actual.Id));
        }

        [Fact]
        public async Task Update_SignStatus_Updated()
        {
            var expected = await registryFixture.CreateVerificationTestRegistryAsync();
            await registryFixture.UpdateRegistrySignStatusAsync(expected.Id, RegistrySignStatus.SignedBySeller);
            
            var actual = await registryFixture.RegistryApi.Put(expected.Id, expected);

            Assert.NotNull(actual);
            Assert.Equal(RegistryStatus.InProcess, actual.Status);
            Assert.Equal(expected.SignStatus, actual.SignStatus);

            var supplies = await registryFixture.RegistryApi.GetSupplies(actual.Id);
            Assert.NotEmpty(supplies);
            Assert.True(supplies.All(s => s.RegistryId == actual.Id));
        }

        //TODO make this work
        // [Fact]
        // public async Task Create_Registry_Generate_Excel()
        // {
        //     var (registry, discount) = await registryFixture.CreateDiscountingTestRegistryAsync();
        //     await registryFixture.RegistryApi.GetFile(registry.Id, TemplateType.Discount);
        // }
    }
}