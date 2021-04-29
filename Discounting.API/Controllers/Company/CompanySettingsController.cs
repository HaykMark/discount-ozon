using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Filters;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Common.AccessControl;
using Discounting.Entities.Auditing;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.CompanyServices;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Company
{
    [ApiVersion("1.0")]
    [Route(Routes.Companies)]
    [Zone(Zones.CompanySettings)]
    public class CompanySettingsController : BaseController
    {
        private readonly IMapper mapper;
        private readonly ICompanySettingsService settingsService;
        private readonly IAuditService auditService;

        public CompanySettingsController(
            IMapper mapper,
            IFirewall firewall,
            ICompanySettingsService settingsService,
            IAuditService auditService
        )
            : base(firewall)
        {
            this.mapper = mapper;
            this.settingsService = settingsService;
            this.auditService = auditService;
        }


        [HttpGet("{id}/settings")]
        public async Task<CompanySettings> GetCompanySettings(Guid id)
        {
            return mapper.Map<CompanySettings>(await settingsService.GetSettings(id));
        }

        [HttpPost("{id}/settings")]
        public async Task<CompanySettings> CreateCompanySettings(Guid id, [FromBody] CompanySettingsDTO settingsDto)
        {
            var companySettings = await settingsService.CreateSettingsAsync(mapper.Map<CompanySettings>(settingsDto));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.CompanySettingsCreated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = companySettings.Id.ToString()
            });
            return mapper.Map<CompanySettings>(companySettings);
        }

        [HttpPut("{id}/settings/{sid}")]
        [DisableRoutValidator]
        public async Task<CompanySettings> UpdateCompanySettings(Guid id, Guid sid,
            [FromBody] CompanySettingsDTO settingsDto)
        {
            var companySettings = await settingsService.UpdateSettingsAsync(mapper.Map<CompanySettings>(settingsDto));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.CompanySettingsUpdated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = companySettings.Id.ToString(),
                Message = await auditService.GetMessageAsync<CompanySettings>(companySettings.Id)
            });
            return mapper.Map<CompanySettings>(companySettings);
        }
    }
}