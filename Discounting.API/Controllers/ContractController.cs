using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Email;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.EmailNotification;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers
{
    [ApiVersion("1.0")]
    [Route(Routes.Contracts)]
    [Zone(Zones.Contracts)]
    public class ContractController : BaseController
    {
        private readonly IContractService contractService;
        private readonly IWebHostEnvironment environment;
        private readonly IMailer mailer;
        private readonly IMapper mapper;
        private readonly IAuditService auditService;

        public ContractController(
            IMapper mapper,
            IContractService contractService,
            IFirewall firewall,
            IWebHostEnvironment environment,
            IMailer mailer,
            IAuditService auditService
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.contractService = contractService;
            this.environment = environment;
            this.mailer = mailer;
            this.auditService = auditService;
        }

        [HttpGet]
        public async Task<ResourceCollection<ContractDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            var (contracts, count) = await contractService.GetAllAsync(offset, limit);
            return new ResourceCollection<ContractDTO>(
                mapper.Map<ResourceCollection<ContractDTO>>(contracts), count);
        }

        [HttpGet("{id}")]
        public async Task<ContractDTO> Get(Guid id)
        {
            return mapper.Map<ContractDTO>(await contractService.GetAsync(id));
        }

        [HttpPost]
        public async Task<ContractDTO> Post([FromBody] ContractDTO model)
        {
            var contractDto = mapper.Map<ContractDTO>(await contractService.CreateAsync(mapper.Map<Contract>(model)));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.ContractCreated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = contractDto.Id.ToString(),
                Message = await auditService.GetMessageAsync<Contract>(contractDto.Id)
            });
            return contractDto;
        }

        [HttpPut("{id}")]
        public async Task<ContractDTO> Put(Guid id, [FromBody] ContractDTO model)
        {
            var contractDto = mapper.Map<ContractDTO>(await contractService.UpdateAsync(mapper.Map<Contract>(model)));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.ContractUpdated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = contractDto.Id.ToString(),
                Message = await auditService.GetMessageAsync<Contract>(contractDto.Id)
            });
            return contractDto;
        }

        [HttpPost("send-email-notification")]
        public async Task<NoContentResult> SendActivation([FromBody] ContractEmailNotificationDTO model)
        {
            var contract = await contractService.GetDetailedAsync(model.Id);
            var html = EmailTemplates.GetContractHtmlString(environment, contract.Buyer.ShortName,
                model.Type, model.ReturnUrl);
            var subject = model.Type == ContractEmailEventType.Updated
                ? $"{contract.Buyer.ShortName} отредактировал сведения по связке."
                : $"{contract.Buyer.ShortName} добавил связку.";
            await mailer.SendEmailAsync(contract.Seller.Users.Select(u => u.Email), subject, html);

            return NoContent();
        }
    }
}