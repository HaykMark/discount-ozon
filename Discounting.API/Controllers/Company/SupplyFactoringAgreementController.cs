using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.AccessControl;
using Discounting.Logics.FactoringAgreements;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Company
{
    [ApiVersion("1.0")]
    [Route(Routes.SupplyFactoringAgreements)]
    [Zone(Zones.SupplyFactoringAgreement)]
    public class SupplyFactoringAgreementController : BaseController
    {
        private readonly IMapper mapper;
        private readonly ISupplyFactoringAgreementService supplyFactoringAgreementService;

        public SupplyFactoringAgreementController(
            IMapper mapper,
            ISupplyFactoringAgreementService supplyFactoringAgreementService,
            IFirewall firewall
        )
            : base(firewall)
        {
            this.mapper = mapper;
            this.supplyFactoringAgreementService = supplyFactoringAgreementService;
        }

        [HttpGet]
        public async Task<ResourceCollection<SupplyFactoringAgreementDTO>> Get(Guid? factoringAgreementId = null)
        {
            return new ResourceCollection<SupplyFactoringAgreementDTO>(
                mapper.Map<ResourceCollection<SupplyFactoringAgreementDTO>>(
                    await supplyFactoringAgreementService.GetAll(factoringAgreementId)));
        }

        [HttpGet("{id}")]
        public async Task<SupplyFactoringAgreementDTO> Get(Guid id)
        {
            return mapper.Map<SupplyFactoringAgreementDTO>(await supplyFactoringAgreementService.Get(id));
        }

        [HttpPost]
        public async Task<SupplyFactoringAgreementDTO> Post([FromBody] SupplyFactoringAgreementDTO dto)
        {
            return mapper.Map<SupplyFactoringAgreementDTO>(
                await supplyFactoringAgreementService.CreateAsync(
                    mapper.Map<SupplyFactoringAgreement>(dto)));
        }
        
        [HttpPut("{id}")]
        public async Task<SupplyFactoringAgreementDTO> Put(Guid id, [FromBody] SupplyFactoringAgreementDTO dto)
        {
            return mapper.Map<SupplyFactoringAgreementDTO>(
                await supplyFactoringAgreementService.UpdateAsync(
                    mapper.Map<SupplyFactoringAgreement>(dto)));
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await supplyFactoringAgreementService.RemoveAsync(id);
            return NoContent();
        }
    }
}