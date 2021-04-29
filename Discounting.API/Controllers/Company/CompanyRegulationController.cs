using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Company;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.CompanyServices;
using Discounting.Logics.Models;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers
{
    [ApiVersion("1.0")]
    [Route(Routes.CompanyRegulations)]
    [Zone(Zones.CompanyRegulations)]
    public class CompanyRegulationController : BaseController
    {
        private readonly ICompanyRegulationService companyRegulationService;
        private readonly IMapper mapper;
        private readonly IUploadPathProviderService pathProviderService;
        private readonly ISignatureService signatureService;
        private readonly IAuditService auditService;

        public CompanyRegulationController(
            IMapper mapper,
            IFirewall firewall,
            ICompanyRegulationService companyRegulationService,
            IUploadPathProviderService pathProviderService,
            ISignatureService signatureService,
            IAuditService auditService
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.companyRegulationService = companyRegulationService;
            this.pathProviderService = pathProviderService;
            this.signatureService = signatureService;
            this.auditService = auditService;
        }

        [HttpGet]
        public async Task<ResourceCollection<CompanyRegulationDTO>> Get(CompanyRegulationType? type = null,
            Guid? companyId = null)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            return mapper.Map<ResourceCollection<CompanyRegulationDTO>>(
                await companyRegulationService.GetAllAsync(type, companyId));
        }

        [HttpGet("{id}")]
        public async Task<CompanyRegulationDTO> Get(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            return mapper.Map<CompanyRegulationDTO>(await companyRegulationService.Get(id));
        }

        [HttpPost]
        public async Task<CompanyRegulationDTO> Post([FromForm] CompanyRegulationRequestDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            var companyRegulation = await companyRegulationService.CreateAsync(model.Type, model.File);
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.CompanyRegulationCreated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = companyRegulation.Id.ToString()
            });
            return mapper.Map<CompanyRegulationDTO>(companyRegulation);
        }

        [HttpPut("{id}")]
        public async Task<CompanyRegulationDTO> Put(Guid id, [FromForm] CompanyRegulationRequestDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            var companyRegulation = await companyRegulationService.UpdateAsync(model.Id, model.Type, model.File);
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.CompanyRegulationUpdated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = companyRegulation.Id.ToString()
            });
            return mapper.Map<CompanyRegulationDTO>(companyRegulation);
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            await companyRegulationService.RemoveAsync(id);
            return new NoContentResult();
        }

        /// <summary>
        ///     Generates template for profile regulation
        /// </summary>
        [HttpGet("profile")]
        public async Task<PhysicalFileResult> GenerateProfileFile()
        {
            var filePath = await companyRegulationService.GetProfileFileAsync();
            return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Анкета.xlsx");
        }

        /// <summary>
        ///     Loads the actual file
        /// </summary>
        [HttpGet(Routes.FileSubRoute)]
        public async Task<PhysicalFileResult> GetFile(Guid id)
        {
            var companyRegulation = await companyRegulationService.Get(id);
            var filePath = pathProviderService.GetCompanyRegulationDestinationPath(companyRegulation.Id,
                companyRegulation.ContentType, companyRegulation.Type);
            return PhysicalFile(filePath, companyRegulation.ContentType, Path.GetFileName(filePath));
        }

        //Signatures
        [HttpGet(Routes.WithSignatures)]
        public async Task<ResourceCollection<CompanyRegulationSignatureDTO>> GetSignatures(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            var entities = (await signatureService.TryGetAsync(SignatureType.CompanyRegulation, id))
                .OfType<CompanyRegulationSignature>()
                .ToArray();
            return new ResourceCollection<CompanyRegulationSignatureDTO>(
                mapper.Map<ResourceCollection<CompanyRegulationSignatureDTO>>(entities));
        }

        [HttpGet(Routes.SignatureFileSubRoute)]
        public async Task<PhysicalFileResult> GetSignatureFile(Guid id, Guid sid)
        {
            var filePath =
                await signatureService.TryGetSignatureLocationAsync(sid, SignatureType.CompanyRegulation, id);
            return PhysicalFile(filePath, "application/octet-stream", Path.GetFileName(filePath));
        }

        [HttpPost(Routes.WithSignatures)]
        public async Task<CompanyRegulationSignatureDTO> Sign(Guid id, [FromForm] SignatureRequestDTO dto)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);

            var signature =
                await signatureService.CreateAsync(id, SignatureType.CompanyRegulation,
                    mapper.Map<SignatureRequest>(dto));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.CompanyRegulationSigned,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = id.ToString()
            });
            return mapper.Map<CompanyRegulationSignatureDTO>((CompanyRegulationSignature) signature);
        }
    }
}