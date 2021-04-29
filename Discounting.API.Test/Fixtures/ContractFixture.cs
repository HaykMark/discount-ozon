using System.Threading.Tasks;
using Discounting.API.Common.ViewModels;
using Discounting.Entities;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class ContractFixture : BaseFixture
    {
        public ContractFixture(AppState appState) : base(appState)
        {
        }

        public ContractDTO GetPayload()
        {
            return new ContractDTO
            {
                SellerId = GuidValues.CompanyGuids.TestSimpleUser,
                BuyerId = GuidValues.CompanyGuids.Admin,
                Provider = ContractProvider.Manually,
                SellerTin = TestConstants.TestSellerTin,
                IsDynamicDiscounting = true
            };
        }

        public Task<ContractDTO> CreateContractAsync(ContractDTO payload = null)
        {
            return ContractApi.Post(payload ?? GetPayload());
        }
    }
}