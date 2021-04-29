using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Email;
using Discounting.API.Common.ViewModels.Company;
using Discounting.API.Common.ViewModels.EmailNotification;
using Discounting.Common.AccessControl;
using Discounting.Common.Exceptions;
using Discounting.Common.Response;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.FactoringAgreements;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Company
{
    [ApiVersion("1.0")]
    [Route(Routes.FactoringAgreements)]
    [Zone(Zones.FactoringAgreement)]
    public class FactoringAgreementController : BaseController
    {
        private readonly IFactoringAgreementService factoringAgreementService;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment environment;
        private readonly IMailer mailer;

        public FactoringAgreementController(
            IMapper mapper,
            IFirewall firewall,
            IFactoringAgreementService factoringAgreementService,
            IWebHostEnvironment environment,
            IMailer mailer
        )
            : base(firewall)
        {
            this.mapper = mapper;
            this.factoringAgreementService = factoringAgreementService;
            this.environment = environment;
            this.mailer = mailer;
        }


        [HttpGet]
        public async Task<ResourceCollection<FactoringAgreementDTO>> Get(
            Guid? companyId = null,
            Guid? bankId = null,
            string supplyNumber = null,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            var (favorites, count) =
                await factoringAgreementService.Get(companyId, bankId, supplyNumber, offset, limit);
            return new ResourceCollection<FactoringAgreementDTO>(
                mapper.Map<ResourceCollection<FactoringAgreementDTO>>(favorites), count);
        }

        [HttpGet("{id}")]
        public async Task<FactoringAgreementDTO> Get(Guid id)
        {
            return mapper.Map<FactoringAgreementDTO>(await factoringAgreementService.Get(id));
        }

        [HttpPost]
        public async Task<FactoringAgreementDTO> Post(Guid id, [FromBody] FactoringAgreementDTO settingsDto)
        {
            return mapper.Map<FactoringAgreementDTO>(
                await factoringAgreementService.CreateAsync(mapper.Map<FactoringAgreement>(settingsDto)));
        }

        [HttpPut("{id}")]
        public async Task<FactoringAgreementDTO> Put(Guid id,
            [FromBody] FactoringAgreementDTO factoringAgreementDto)
        {
            return mapper.Map<FactoringAgreementDTO>(
                await factoringAgreementService.UpdateAsync(mapper.Map<FactoringAgreement>(factoringAgreementDto)));
        }

        [HttpPost("send-email-notification")]
        public async Task<NoContentResult> SendActivation([FromBody] FactoringAgreementNotificationDTO model)
        {
            var factoringAgreement = await factoringAgreementService.GetDetailedAsync(model.Id);
            await mailer.SendFactoringAgreementEmailAsync(factoringAgreement, model.Type, model.ReturnUrl);

            return NoContent();
        }
    }
}