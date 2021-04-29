using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Entities.TariffDiscounting;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class DiscountSettingsFixture : BaseFixture
    {
        public DiscountSettingsFixture(AppState appState) : base(appState)
        {
        }

        public Task<DiscountSettingsDTO> CreateTestDiscountSettingsAsync(DiscountSettingsDTO payload = null)
        {
            payload ??= GetPayload();
            return DiscountSettingsApi.Post(payload);
        }

        public DiscountSettingsDTO GetPayload(Guid? companyId = null)
        {
            return new DiscountSettingsDTO
            {
                CompanyId = companyId ?? GuidValues.CompanyGuids.TestBuyer,
                DaysType = DaysType.Business,
                PaymentWeekDays = DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday | DaysOfWeek.Thursday |
                                  DaysOfWeek.Friday | DaysOfWeek.Saturday | DaysOfWeek.Sunday,
                MinimumDaysToShift = 3
            };
        }
    }
}