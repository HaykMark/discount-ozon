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
    public class CompanyAuthorizedUserController : BaseController
    {
        private readonly ICompanyAggregateService companyAggregateService;
        private readonly IMapper mapper;

        public CompanyAuthorizedUserController(
            IMapper mapper,
            ICompanyAggregateService companyAggregateService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.companyAggregateService = companyAggregateService;
        }

        [HttpGet("{id}/authorized-user")]
        public async Task<CompanyAuthorizedUserInfoDTO> Get(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var infos = await companyAggregateService.Get<CompanyAuthorizedUserInfo>(id);
            return mapper.Map<CompanyAuthorizedUserInfoDTO>(infos.First());
        }

        [HttpPost("{id}/authorized-user")]
        public async Task<CompanyAuthorizedUserInfoDTO> Post([FromBody] CompanyAuthorizedUserInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<CompanyAuthorizedUserInfo>(dto);
            entity = await companyAggregateService.CreateAsync(entity);
            return mapper.Map<CompanyAuthorizedUserInfoDTO>(entity);
        }

        [HttpPut("{id}/authorized-user/{sid}")]
        [DisableRoutValidator]
        public async Task<CompanyAuthorizedUserInfoDTO> Put(Guid id, Guid sid,
            [FromBody] CompanyAuthorizedUserInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<CompanyAuthorizedUserInfo>(dto);
            entity = await companyAggregateService.UpdateAsync(entity);
            return mapper.Map<CompanyAuthorizedUserInfoDTO>(entity);
        }
    }
}