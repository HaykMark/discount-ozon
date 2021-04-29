using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Registry;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Entities;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class DiscountedRegistryTests : TestBase
    {
        public DiscountedRegistryTests(AppState appState) : base(appState)
        {
            registryFixture = new RegistryFixture(appState);
        }

        private readonly RegistryFixture registryFixture;

        [Fact]
        public async Task Create_Confirm_Confirmed()
        {
            var (registry, discount) = await registryFixture.CreateDiscountingTestRegistryAsync();
            registry.SignStatus = RegistrySignStatus.SignedBySeller;
            await registryFixture.RegistryApi.Put(registry.Id, registry);

            await registryFixture.LogoutAsync();
            await registryFixture.LoginBuyerAsync();

            var updatedDiscount = await registryFixture.DiscountApi.Put(discount.Id, discount);
            Assert.NotNull(updatedDiscount);
            Assert.Equal(discount.Id, updatedDiscount.Id);
            Assert.Equal(discount.Rate, updatedDiscount.Rate);

            var conformedRegistry = await registryFixture.RegistryApi.Get(registry.Id);
            Assert.NotNull(conformedRegistry);
            Assert.Equal(registry.Id, conformedRegistry.Id);
            Assert.True(conformedRegistry.IsConfirmed);
            Assert.True(conformedRegistry.SignStatus == registry.SignStatus);
        }

        [Fact]
        public async Task Create_Confirm_Change_Rate_Confirmed_SignStatus_NotSigned()
        {
            var (registry, discount) = await registryFixture.CreateDiscountingTestRegistryAsync();
            registry.SignStatus = RegistrySignStatus.SignedBySeller;
            await registryFixture.RegistryApi.Put(registry.Id, registry);

            await registryFixture.LogoutAsync();
            await registryFixture.LoginBuyerAsync();
            discount.Rate = 10;
            discount.HasChanged = true;

            var updatedDiscount = await registryFixture.DiscountApi.Put(discount.Id, discount);
            Assert.NotNull(updatedDiscount);
            Assert.Equal(discount.Id, updatedDiscount.Id);
            Assert.Equal(discount.Rate, updatedDiscount.Rate);
            Assert.True(updatedDiscount.HasChanged);

            var conformedRegistry = await registryFixture.RegistryApi.Get(registry.Id);
            Assert.NotNull(conformedRegistry);
            Assert.Equal(registry.Id, conformedRegistry.Id);
            Assert.True(conformedRegistry.IsConfirmed);
            Assert.True(conformedRegistry.SignStatus == RegistrySignStatus.NotSigned);
        }

        [Fact]
        public async Task Create_GetAll_Buyer_Empty()
        {
            var (registry, discount) = await registryFixture.CreateDiscountingTestRegistryAsync();
            await registryFixture.LogoutAsync();
            await registryFixture.LoginBuyerAsync();
            var inProcessRegistriesBuyer = await registryFixture.RegistryApi.GetInProcess();
            Assert.Empty(inProcessRegistriesBuyer);
        }

        [Fact]
        public async Task Create_GetAll_Seller_NotEmpty()
        {
            var (registry, discount) = await registryFixture.CreateDiscountingTestRegistryAsync();
            Assert.NotNull(discount);
            Assert.Equal(registry.Id, discount.RegistryId);

            var discountRequestDto = await registryFixture.RegistryApi.GetDiscount(registry.Id);
            Assert.Equal(discount.Id, discountRequestDto.Id);
            Assert.Equal(registry.Id, discountRequestDto.RegistryId);

            var supplyDiscounts = await registryFixture.RegistryApi.GetSupplyDiscounts(registry.Id);
            Assert.NotEmpty(supplyDiscounts);

            var inProcessRegistriesSeller = await registryFixture.RegistryApi.GetInProcess();
            Assert.NotEmpty(inProcessRegistriesSeller);
            Assert.NotEmpty(inProcessRegistriesSeller.Where(r => r.Id == registry.Id));

            var financedSupplies = await registryFixture.SupplyApi.GetInFinance();
            Assert.NotEmpty(financedSupplies);
            Assert.True(financedSupplies.All(s => s.RegistryId == registry.Id));

            foreach (var supplyDiscount in supplyDiscounts)
                Assert.Contains(financedSupplies, s => s.Id == supplyDiscount.SupplyId);
        }

        [Fact]
        public async Task Create_SetSupplies()
        {
            var (registry, discount) = await registryFixture.CreateDiscountingTestRegistryAsync();
            var registrySupplies = await registryFixture.RegistryApi.GetSupplies(registry.Id);

            registrySupplies.RemoveAt(registrySupplies.Count - 1);

            var updatedRegistry =
                await registryFixture.RegistryApi.SetSupplies(registry.Id,
                    registrySupplies.Select(s => s.Id).ToArray());

            var supplyDiscounts = await registryFixture.RegistryApi.GetSupplyDiscounts(registry.Id);

            var updatedDiscount = await registryFixture.RegistryApi.GetDiscount(registry.Id);
            Assert.NotEqual(registry.Amount, updatedRegistry.Amount);
            Assert.NotEqual(discount.AmountToPay, updatedDiscount.AmountToPay);
            Assert.NotEqual(discount.Rate, updatedDiscount.Rate);
            Assert.NotEqual(discount.DiscountedAmount, updatedDiscount.DiscountedAmount);
            Assert.Equal(registrySupplies.Count, supplyDiscounts.Count);
        }

        [Fact]
        public async Task Create_SignBySeller_GetAll_Buyer_NotEmpty()
        {
            var (registry, discount) = await registryFixture.CreateDiscountingTestRegistryAsync();
            registry.SignStatus = RegistrySignStatus.SignedBySeller;
            await registryFixture.RegistryApi.Put(registry.Id, registry);
            await registryFixture.LogoutAsync();
            await registryFixture.LoginBuyerAsync();
            var inProcessRegistriesBuyer = await registryFixture.RegistryApi.GetInProcess();
            Assert.NotEmpty(inProcessRegistriesBuyer);
            Assert.NotEmpty(inProcessRegistriesBuyer.Where(r => r.Id == registry.Id));

            var financedSupplies = await registryFixture.SupplyApi.GetInFinance();
            Assert.NotEmpty(financedSupplies);
            Assert.True(financedSupplies.All(s => s.RegistryId == registry.Id));
        }

        [Fact]
        public async Task Create_Discount_WrongPaymentDate_RegistryRemoved()
        {
            await registryFixture.LoginBuyerAsync();

            var contract = await registryFixture.ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            contract.IsDynamicDiscounting = true;
            await registryFixture.ContractApi.Put(contract.Id, contract);
            await registryFixture.DiscountSettingsFixture.CreateTestDiscountSettingsAsync();

            var supplies = registryFixture.SupplyFixture.MainSuppliesPayload();
            var suppliesResponseDto = await registryFixture.SupplyFixture.SupplyApi.Post(supplies);
            await registryFixture.LogoutAsync();
            await registryFixture.LoginSellerAsync();
            var suppliesIds = suppliesResponseDto.Supplies.Select(s => s.Id).ToArray();
            var requestDto = new RegistryRequestDTO
            {
                SupplyIds = suppliesIds,
                FinanceType = FinanceType.DynamicDiscounting
            };
            var registryDto = await registryFixture.RegistryApi.Post(requestDto);
            await AssertHelper.AssertUnprocessableEntityAsync(() =>
                registryFixture.DiscountFixture.CreateDiscountAsync(registryDto, false, DateTime.UtcNow.AddDays(1)));
            await AssertHelper.AssertNotFoundAsync(() => registryFixture.RegistryApi.Get(registryDto.Id));
        }

        [Fact]
        public async Task Create_DiscountWithDiscountSupplies_WrongUser_DiscountAndRegistryRemoved()
        {
            await registryFixture.LoginBuyerAsync();

            var contract = await registryFixture.ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            contract.IsDynamicDiscounting = true;
            await registryFixture.ContractApi.Put(contract.Id, contract);
            await registryFixture.DiscountSettingsFixture.CreateTestDiscountSettingsAsync();

            var supplies = registryFixture.SupplyFixture.MainSuppliesPayload();
            var suppliesResponseDto = await registryFixture.SupplyFixture.SupplyApi.Post(supplies);
            await registryFixture.LogoutAsync();
            await registryFixture.LoginSellerAsync();
            var suppliesIds = suppliesResponseDto.Supplies.Select(s => s.Id).ToArray();
            var requestDto = new RegistryRequestDTO
            {
                SupplyIds = suppliesIds,
                FinanceType = FinanceType.DynamicDiscounting
            };
            var registryDto = await registryFixture.RegistryApi.Post(requestDto);
            var discountDto =
                await registryFixture.DiscountFixture.CreateDiscountAsync(registryDto, false);
            var supplyDiscountDtos = suppliesIds
                .Select(s => new SupplyDiscountDTO
                {
                    Rate = 5,
                    DiscountedAmount = 10,
                    SupplyId = s
                })
                .ToArray();
            await registryFixture.LoginBankAsync();
            await AssertHelper.AssertForbiddenAsync(() =>
                registryFixture.DiscountApi.CreateSupplyDiscount(discountDto.Id, supplyDiscountDtos));
            await AssertHelper.AssertNotFoundAsync(() => registryFixture.RegistryApi.Get(registryDto.Id));
            await AssertHelper.AssertNotFoundAsync(() => registryFixture.DiscountApi.Get(discountDto.Id));
            await registryFixture.LoginSellerAsync();
            var updatedSupply = await registryFixture.SupplyApi.Get(suppliesIds.First());
            Assert.Equal(SupplyStatus.InProcess, updatedSupply.Status);
        }
    }
}