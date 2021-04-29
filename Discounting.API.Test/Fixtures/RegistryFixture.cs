using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Registry;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Entities;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class RegistryFixture : BaseFixture
    {
        public RegistryFixture(AppState appState) : base(appState)
        {
        }

        public async Task<RegistryDTO> CreateVerificationTestRegistryAsync(RegistryRequestDTO payload = null)
        {
            await LoginBuyerAsync();
            if (payload != null) return await RegistryApi.Post(payload);

            var contract = await ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            contract.IsRequiredRegistry = true;
            contract.IsRequiredNotification = true;
            await ContractApi.Put(contract.Id, contract);

            var supplies = SupplyFixture.MainSuppliesPayload();
            var supplyDto = await SupplyFixture.SupplyApi.Post(supplies);
            await LogoutAsync();
            await LoginSellerAsync();

            //Create FA and SFA
            var factoringAgreementPayload = FactoringAgreementFixture.GetPayload(GuidValues.CompanyGuids.TestSeller);
            factoringAgreementPayload.BankId = GuidValues.CompanyGuids.BankUserSecond;
            var factoringAgreementDto =
                await FactoringAgreementApi.Create(factoringAgreementPayload);
            //Confirm FA
            await LogoutAsync();
            await LoginSecondBankAsync();
            factoringAgreementDto.IsConfirmed = true;
            await FactoringAgreementApi.Update(factoringAgreementDto.Id, factoringAgreementDto);
            
            //Create registry
            await LogoutAsync();
            await LoginSellerAsync();

            var requestDto = new RegistryRequestDTO
            {
                SupplyIds = supplyDto.Supplies.Select(s => s.Id).ToArray(),
                FinanceType = FinanceType.SupplyVerification,
                BankId = GuidValues.CompanyGuids.BankUserSecond,
                FactoringAgreementId = factoringAgreementDto.Id
            };
            return await RegistryApi.Post(requestDto);
        }

        public async Task<(RegistryDTO, DiscountDTO)> CreateDiscountingTestRegistryAsync()
        {
            await LoginBuyerAsync();

            var contract = await ContractApi.Get(GuidValues.ContractGuids.TestBuyer);
            contract.IsDynamicDiscounting = true;
            await ContractApi.Put(contract.Id, contract);
            await DiscountSettingsFixture.CreateTestDiscountSettingsAsync();

            var supplies = SupplyFixture.MainSuppliesPayload();
            var suppliesResponseDto = await SupplyFixture.SupplyApi.Post(supplies);
            await LogoutAsync();
            await LoginSellerAsync();
            var suppliesIds = suppliesResponseDto.Supplies.Select(s => s.Id).ToArray();

            var requestDto = new RegistryRequestDTO
            {
                SupplyIds = suppliesIds,
                FinanceType = FinanceType.DynamicDiscounting
            };
            var registryDto = await RegistryApi.Post(requestDto);
            var discountDto = await DiscountFixture.CreateDiscountAsync(registryDto, false);
            var supplyDiscountDtos = suppliesIds
                .Select(s => new SupplyDiscountDTO
                {
                    Rate = 5,
                    DiscountedAmount = 10,
                    SupplyId = s
                })
                .ToArray();
            await DiscountApi.CreateSupplyDiscount(discountDto.Id, supplyDiscountDtos);
            return (registryDto, discountDto);
        }

        public async Task UpdateRegistryStatusAsync(Guid id, RegistryStatus status)
        {
            var entity = await DbContext.Set<Registry>().FindAsync(id);
            entity.Status = status;
            DbContext.Set<Registry>().Update(entity);
            await DbContext.SaveChangesAsync();
        }

        public async Task UpdateRegistrySignStatusAsync(Guid id, RegistrySignStatus status)
        {
            var entity = await DbContext.Set<Registry>().FindAsync(id);
            entity.SignStatus = status;
            DbContext.Set<Registry>().Update(entity);
            await DbContext.SaveChangesAsync();
        }
    }
}