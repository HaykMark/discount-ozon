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
    public class MigrationCardController : BaseController
    {
        private readonly ICompanyAggregateService companyAggregateService;
        private readonly IMapper mapper;

        public MigrationCardController(
            IMapper mapper,
            ICompanyAggregateService companyAggregateService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.companyAggregateService = companyAggregateService;
        }

        [HttpGet("{id}/migration-cards")]
        public async Task<ResourceCollection<MigrationCardInfoDTO>> Get(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var infos = await companyAggregateService.Get<MigrationCardInfo>(id);
            return mapper.Map<ResourceCollection<MigrationCardInfoDTO>>(infos);
        }

        [HttpPost("{id}/migration-cards")]
        public async Task<MigrationCardInfoDTO> Post([FromBody] MigrationCardInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<MigrationCardInfo>(dto);
            entity = await companyAggregateService.CreateAsync(entity);
            return mapper.Map<MigrationCardInfoDTO>(entity);
        }

        [HttpPut("{id}/migration-cards/{sid}")]
        [DisableRoutValidator]
        public async Task<MigrationCardInfoDTO> Put(Guid id, Guid sid,
            [FromBody] MigrationCardInfoDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var entity = mapper.Map<MigrationCardInfo>(dto);
            entity = await companyAggregateService.UpdateAsync(entity);
            return mapper.Map<MigrationCardInfoDTO>(entity);
        }
    }
}