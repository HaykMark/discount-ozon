using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface ICalendarApi
    {
        [Get("/api/calendar/free-days")]
        Task<List<FreeDayDTO>> Get(
            DateTime? from = null,
            DateTime? until = null,
            bool withDeactivated = false);

        [Post("/api/calendar/free-days")]
        Task<List<FreeDayDTO>> Post(List<FreeDayDTO> dto);
    }
}