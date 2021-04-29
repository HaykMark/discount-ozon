using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Templates;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Data.Context;
using Discounting.Entities.Auditing;
using Discounting.Entities.Templates;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Templates;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Templates
{
    [ApiVersion("1.0")]
    [Route(Routes.BuyerTemplateConnections)]
    [Zone(Zones.BuyerTemplateConnections)]
    public class BuyerTemplateConnectionController : BaseController
    {
        private readonly IBuyerTemplateConnectionService buyerTemplateConnectionService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuditService auditService;

        public BuyerTemplateConnectionController(
            IBuyerTemplateConnectionService buyerTemplateConnectionService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IFirewall firewall,
            IAuditService auditService
        )
            : base(firewall)
        {
            this.buyerTemplateConnectionService = buyerTemplateConnectionService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.auditService = auditService;
        }

        [HttpGet]
        public async Task<ResourceCollection<BuyerTemplateConnectionDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            var (buyerTemplateConnections, count) = await buyerTemplateConnectionService.GetAll(offset, limit);
            return new ResourceCollection<BuyerTemplateConnectionDTO>(
                mapper.Map<ResourceCollection<BuyerTemplateConnectionDTO>>(buyerTemplateConnections), count);
        }

        [HttpGet("{id}")]
        public async Task<BuyerTemplateConnectionDTO> Get(Guid id)
        {
            return mapper.Map<BuyerTemplateConnectionDTO>(await buyerTemplateConnectionService.Get(id));
        }

        [HttpPost]
        public async Task<BuyerTemplateConnectionDTO> Post([FromBody] BuyerTemplateConnectionDTO model)
        {
            var buyerTemplateConnection = await buyerTemplateConnectionService.CreateAsync(mapper.Map<BuyerTemplateConnection>(model));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.TemplatedAddedToBuyer,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = buyerTemplateConnection.Id.ToString(),
                Message = await auditService.GetMessageAsync<BuyerTemplateConnection>(buyerTemplateConnection.Id)
            });
            return mapper.Map<BuyerTemplateConnectionDTO>(buyerTemplateConnection);
        }

        [HttpPut("{id}")]
        public async Task<BuyerTemplateConnectionDTO> Put(Guid id, [FromBody] BuyerTemplateConnectionDTO model)
        {
            var buyerTemplateConnection = await buyerTemplateConnectionService.UpdateAsync(mapper.Map<BuyerTemplateConnection>(model));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.TemplateForBuyerUpdated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = buyerTemplateConnection.Id.ToString(),
                Message = await auditService.GetMessageAsync<BuyerTemplateConnection>(buyerTemplateConnection.Id)
            });
            return mapper.Map<BuyerTemplateConnectionDTO>(buyerTemplateConnection);
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await unitOfWork.RemoveAndSaveAsync<BuyerTemplateConnection, Guid>(id);
            return NoContent();
        }
    }
}