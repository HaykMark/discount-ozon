using System.Threading.Tasks;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.Regulations;
using Discounting.Entities;
using Discounting.Entities.Regulations;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class RegulationFixture : BaseFixture
    {
        public RegulationFixture(AppState appState) : base(appState)
        {
        }

        public Task<RegulationDTO> CreateTestRegulationAsync(RegulationDTO regulationDto = null)
        {
            return RegulationApi.Post(regulationDto ?? GetPayload());
        }

        public RegulationDTO GetPayload(RegulationType type = RegulationType.ConditionsRegulationsPolicy)
        {
            return new RegulationDTO
            {
                Type = type
            };
        }
    }
}