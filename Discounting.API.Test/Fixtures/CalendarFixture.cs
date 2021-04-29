using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class CalendarFixture : BaseFixture
    {
        public CalendarFixture(AppState appState) : base(appState)
        {
        }

        public Task<List<FreeDayDTO>> CreateFreeDaysAsync(List<FreeDayDTO> payload = null)
        {
            payload ??= GetFreeDaysPayload();
            return CalendarApi.Post(payload);
        }

        public List<FreeDayDTO> GetFreeDaysPayload()
        {
            return new List<FreeDayDTO>
            {
                new FreeDayDTO
                {
                    Date = new DateTime(2020, 03, 08),
                    IsActive = true
                },
                new FreeDayDTO
                {
                    Date = new DateTime(2020, 05, 09),
                    IsActive = true
                }
            };
        }
    }
}