using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Filters;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Common.AccessControl;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.AccessControl;
using Discounting.Logics.CompanyServices;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Company
{
    [ApiVersion("1.0")]
    [Route(Routes.Companies)]
    [Zone(Zones.Companies)]
    public class CompanyContactController : BaseController
    {
        private readonly ICompanyAggregateService companyAggregateService;
        private readonly IMapper mapper;

        public CompanyContactController(
            IMapper mapper,
            ICompanyAggregateService companyAggregateService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.companyAggregateService = companyAggregateService;
        }

        [HttpGet("{id}/contact")]
        public async Task<CompanyContactInfoDTO> Get(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var infos = await companyAggregateService.Get<CompanyContactInfo>(id);
            return mapper.Map<CompanyContactInfoDTO>(infos.First());
        }

        [HttpPost("{id}/contact")]
        public async Task<CompanyContactInfoDTO> Post([FromBody] CompanyContactInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<CompanyContactInfo>(dto);
            entity = await companyAggregateService.CreateAsync(entity);
            return mapper.Map<CompanyContactInfoDTO>(entity);
        }

        [HttpPut("{id}/contact/{sid}")]
        [DisableRoutValidator]
        public async Task<CompanyContactInfoDTO> Put(Guid id, Guid sid,
            [FromBody] CompanyContactInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<CompanyContactInfo>(dto);
            entity = await companyAggregateService.UpdateAsync(entity);
            return mapper.Map<CompanyContactInfoDTO>(entity);
        }
    }
}