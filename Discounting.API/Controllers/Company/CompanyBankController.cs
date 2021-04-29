using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
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
    public class CompanyBankController : BaseController
    {
        private readonly IMapper mapper;
        private readonly ICompanyBankService companyBankService;
        public CompanyBankController(
            IMapper mapper,
            IFirewall firewall, ICompanyBankService companyBankService) : base(firewall)
        {
            this.mapper = mapper;
            this.companyBankService = companyBankService;
        }

        [HttpGet("{id}/bank")]
        public async Task<CompanyBankInfoDTO> Get(Guid id)
        {
            return mapper.Map<CompanyBankInfoDTO>(await companyBankService.GetAsync(id));
        }

        [HttpPost("{id}/bank")]
        public async Task<CompanyBankInfoDTO> Post([FromBody] CompanyBankInfoDTO dto)
        {
            var entity = mapper.Map<CompanyBankInfo>(dto);
            entity = await companyBankService.CreateOrUpdateAsync(entity);
            return mapper.Map<CompanyBankInfoDTO>(entity);
        }
    }
}