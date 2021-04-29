using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers
{
    [ApiVersion("1.0")]
    [Route(Routes.Calendar)]
    [Zone(Zones.Calendar)]
    public class CalendarController : BaseController
    {
        private readonly ICalendarService calendarService;
        private readonly IMapper mapper;

        public CalendarController(
            IMapper mapper,
            ICalendarService calendarService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.calendarService = calendarService;
        }

        [HttpGet("free-days")]
        public async Task<ResourceCollection<FreeDay>> Get(
            DateTime? from = null,
            DateTime? until = null,
            bool withDeactivated = false
        )
        {
            return mapper.Map<ResourceCollection<FreeDay>>(
                await calendarService.GetFreeDaysAsync(from, until, withDeactivated));
        }

        [HttpPost("free-days")]
        public async Task<ResourceCollection<FreeDayDTO>> Post([FromBody] List<FreeDayDTO> model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            return mapper.Map<ResourceCollection<FreeDayDTO>>(
                await calendarService.SaveFreeDaysAsync(mapper.Map<List<FreeDay>>(model)));
        }
    }
}