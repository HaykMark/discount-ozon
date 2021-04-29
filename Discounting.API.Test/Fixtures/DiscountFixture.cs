using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Registry;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Entities.TariffDiscounting;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace Discounting.Tests.Fixtures
{
    public class DiscountFixture : BaseFixture
    {
        public DiscountFixture(AppState appState) : base(appState)
        {
        }

        public async Task<DiscountDTO> CreateDiscountAsync(RegistryDTO registryDto, bool hasChanged, DateTime? paymentDate = null)
        {
            return await DiscountApi.Post(
                new DiscountDTO
                {
                    RegistryId = registryDto.Id,
                    PlannedPaymentDate = paymentDate ?? DateTime.UtcNow.AddDays(5),
                    Rate = 8,
                    DiscountedAmount = 100,
                    DiscountingSource = DiscountingSource.Personal,
                    AmountToPay = registryDto.Amount - 100,
                    HasChanged = hasChanged
                });
        }
    }
}