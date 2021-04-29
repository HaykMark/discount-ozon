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
    public class CompanyOwnerPositionController : BaseController
    {
        private readonly ICompanyAggregateService companyAggregateService;
        private readonly IMapper mapper;

        public CompanyOwnerPositionController(
            IMapper mapper,
            ICompanyAggregateService companyAggregateService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.companyAggregateService = companyAggregateService;
        }

        [HttpGet("{id}/owner-position")]
        public async Task<CompanyOwnerPositionInfoDTO> Get(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var infos = await companyAggregateService.Get<CompanyOwnerPositionInfo>(id);
            return mapper.Map<CompanyOwnerPositionInfoDTO>(infos.First());
        }

        [HttpPost("{id}/owner-position")]
        public async Task<CompanyOwnerPositionInfoDTO> Post([FromBody] CompanyOwnerPositionInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<CompanyOwnerPositionInfo>(dto);
            entity = await companyAggregateService.CreateAsync(entity);
            return mapper.Map<CompanyOwnerPositionInfoDTO>(entity);
        }

        [HttpPut("{id}/owner-position/{sid}")]
        [DisableRoutValidator]
        public async Task<CompanyOwnerPositionInfoDTO> Put(Guid id, Guid sid,
            [FromBody] CompanyOwnerPositionInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<CompanyOwnerPositionInfo>(dto);
            entity = await companyAggregateService.UpdateAsync(entity);
            return mapper.Map<CompanyOwnerPositionInfoDTO>(entity);
        }
    }
}