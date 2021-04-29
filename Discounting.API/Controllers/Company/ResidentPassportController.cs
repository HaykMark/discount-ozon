using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Filters;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.AccessControl;
using Discounting.Logics.CompanyServices;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Company
{
    [ApiVersion("1.0")]
    [Route(Routes.Companies)]
    [Zone(Zones.Companies)]
    public class ResidentPassportController : BaseController
    {
        private readonly ICompanyAggregateService companyAggregateService;
        private readonly IMapper mapper;

        public ResidentPassportController(
            IMapper mapper,
            ICompanyAggregateService companyAggregateService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.companyAggregateService = companyAggregateService;
        }

        [HttpGet("{id}/resident-passports")]
        public async Task<ResourceCollection<ResidentPassportInfoDTO>> Get(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var infos = await companyAggregateService.Get<ResidentPassportInfo>(id);
            return mapper.Map<ResourceCollection<ResidentPassportInfoDTO>>(infos);
        }

        [HttpPost("{id}/resident-passports")]
        public async Task<ResidentPassportInfoDTO> Post([FromBody] ResidentPassportInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<ResidentPassportInfo>(dto);
            entity = await companyAggregateService.CreateAsync(entity);
            return mapper.Map<ResidentPassportInfoDTO>(entity);
        }

        [HttpPut("{id}/resident-passports/{sid}")]
        [DisableRoutValidator]
        public async Task<ResidentPassportInfoDTO> Put(Guid id, Guid sid,
            [FromBody] ResidentPassportInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<ResidentPassportInfo>(dto);
            entity = await companyAggregateService.UpdateAsync(entity);
            return mapper.Map<ResidentPassportInfoDTO>(entity);
        }
    }
}