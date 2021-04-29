using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Templates;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class BuyerTemplateConnectionFixture : BaseFixture
    {
        public BuyerTemplateConnectionFixture(AppState appState) : base(appState)
        {
        }

        public async Task<BuyerTemplateConnectionDTO> CreateTestConnectionAsync(
            BuyerTemplateConnectionDTO payload = null)
        {
            payload ??= GetPayload();
            //await BuyerTemplateConnectionApi.
            await BuyerTemplateConnectionApi.Delete(GuidValues.BuyerTemplateConnectionGuids.TestBuyerRegistry);
            return await BuyerTemplateConnectionApi.Post(payload);
        }

        public BuyerTemplateConnectionDTO GetPayload()
        {
            return new BuyerTemplateConnectionDTO
            {
                BuyerId = GuidValues.CompanyGuids.TestBuyer,
                TemplateId = GuidValues.TemplateGuids.Registry
            };
        }
    }
}