using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Email;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.EmailNotification;
using Discounting.API.Common.ViewModels.Registry;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Common.AccessControl;
using Discounting.Common.Exceptions;
using Discounting.Common.Response;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Entities.TariffDiscounting;
using Discounting.Entities.Templates;
using Discounting.Helpers;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discounting.API.Controllers
{
    [ApiVersion("1.0")]
    [Route(Routes.Registries)]
    [Zone(Zones.Registries)]
    public class RegistryController : BaseController
    {
        private readonly IMailer mailer;
        private readonly IMapper mapper;
        private readonly IRegistryService registryService;
        private readonly ISignatureService signatureService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuditService auditService;

        public RegistryController(
            IRegistryService registryService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IFirewall firewall,
            ISignatureService signatureService,
            IMailer mailer,
            IAuditService auditService
        ) : base(firewall)
        {
            this.registryService = registryService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.signatureService = signatureService;
            this.mailer = mailer;
            this.auditService = auditService;
        }

        [HttpGet("{id}")]
        public async Task<RegistryDTO> Get(Guid id)
        {
            return mapper.Map<RegistryDTO>(await unitOfWork.GetOrFailAsync<Registry, Guid>(id));
        }


        [HttpGet("in-process")]
        public async Task<ResourceCollection<RegistryDTO>> GetInProcess(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(Operations.Read, Zones.RegistriesInProcess);
            var (registries, count) = await registryService.GetAsync(offset, limit, RegistryStatus.InProcess);
            return new ResourceCollection<RegistryDTO>(mapper.Map<ResourceCollection<RegistryDTO>>(registries), count);
        }

        [HttpGet("finished")]
        public async Task<ResourceCollection<RegistryDTO>> GetFinished(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(Operations.Read, Zones.RegistriesFinished);
            var (registries, count) = await registryService.GetAsync(offset, limit, RegistryStatus.Finished);
            return new ResourceCollection<RegistryDTO>(mapper.Map<ResourceCollection<RegistryDTO>>(registries), count);
        }

        [HttpGet("declined")]
        public async Task<ResourceCollection<RegistryDTO>> GetNotAvailable(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(Operations.Read, Zones.RegistriesCanceled);
            var (registries, count) = await registryService.GetAsync(offset, limit, RegistryStatus.Declined);
            return new ResourceCollection<RegistryDTO>(mapper.Map<ResourceCollection<RegistryDTO>>(registries), count);
        }

        [HttpPost]
        public async Task<RegistryDTO> Post([FromBody] RegistryRequestDTO registryRequestDto)
        {
            var registry = await registryService.CreateAsync(registryRequestDto.SupplyIds,
                registryRequestDto.FinanceType,
                registryRequestDto.BankId, registryRequestDto.FactoringAgreementId);
            if(registry.FinanceType == FinanceType.SupplyVerification)
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.RegistryCreated,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    SourceId = registry.Id.ToString(),
                    Message = await auditService.GetMessageAsync<Registry>(registry.Id)
                });
            }

            return mapper.Map<RegistryDTO>(registry);
        }

        [HttpPut("{id}")]
        public async Task<RegistryDTO> Put(Guid id, [FromBody] RegistryDTO model)
        {
            var registry = await registryService.UpdateAsync(mapper.Map<Registry>(model));
            if (model.FinanceType == FinanceType.SupplyVerification)
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = model.Status == RegistryStatus.Declined
                        ? IncidentType.RegistryDeclined
                        : IncidentType.RegistryUpdated,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    SourceId = registry.Id.ToString(),
                    Message = await auditService.GetMessageAsync<Registry>(registry.Id)
                });
            }
            else if (registry.Status == RegistryStatus.Declined)
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.DiscountRegistryDeclined,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    SourceId = registry.Id.ToString(),
                    Message = await auditService.GetMessageAsync<Registry>(registry.Id)
                });
            }

            return mapper.Map<RegistryDTO>(registry);
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await registryService.RemoveAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/set-supplies")]
        public async Task<RegistryDTO> SetSupplies(Guid id, [FromBody] Guid[] supplyIds)
        {
            var registry = await registryService.SetSuppliesAsync(id, supplyIds);
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = registry.FinanceType == FinanceType.SupplyVerification
                    ? IncidentType.RegistryUpdated
                    : IncidentType.DiscountRegistryUpdated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = registry.Id.ToString(),
                Message = registry.FinanceType == FinanceType.SupplyVerification
                    ? await auditService.GetMessageAsync<Registry>(registry.Id)
                    : await auditService.GetMessageAsync<Discount>(registry.Discount.Id)
            });
            return mapper.Map<RegistryDTO>(registry);
        }

        [HttpGet("{id}/supplies")]
        public async Task<ResourceCollection<SupplyDTO>> GetSupplies(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            var (supplies, count) = await registryService.GetRegistrySuppliesAsync(id, offset, limit);
            return new ResourceCollection<SupplyDTO>(mapper.Map<ResourceCollection<SupplyDTO>>(supplies), count);
        }

        [HttpGet("{id}/discount")]
        public async Task<DiscountDTO> GetDiscount(Guid id)
        {
            var discount = await unitOfWork.Set<Discount>()
                .FirstOrDefaultAsync(d => d.RegistryId == id);
            if (discount == null)
            {
                throw new NotFoundException(typeof(Discount));
            }

            return mapper.Map<DiscountDTO>(discount);
        }

        [HttpGet("{id}/supply-discounts")]
        public async Task<ResourceCollection<SupplyDiscountDTO>> GetSupplyDiscounts(Guid id)
        {
            var supplyDiscount = await unitOfWork.Set<Supply>()
                .Include(s => s.SupplyDiscount)
                .Where(s => s.RegistryId == id && s.Registry.FinanceType == FinanceType.DynamicDiscounting)
                .Select(s => s.SupplyDiscount)
                .ToListAsync();
            return mapper.Map<ResourceCollection<SupplyDiscountDTO>>(supplyDiscount);
        }

        [HttpGet(Routes.FileSubRoute)]
        public async Task<PhysicalFileResult> GetFile(Guid id, TemplateType type)
        {
            var filePath = await registryService.GetRegistryFileAsync(id, type);
            return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Path.GetFileName(filePath));
        }

        [HttpGet("{id}/signatures")]
        public async Task<ResourceCollection<RegistrySignatureDTO>> GetSignatures(Guid id)
        {
            var entities = await registryService.GetRegistrySignaturesAsync(id);
            return new ResourceCollection<RegistrySignatureDTO>(
                mapper.Map<ResourceCollection<RegistrySignatureDTO>>(entities));
        }

        [HttpGet(Routes.SignatureFileSubRoute)]
        public async Task<PhysicalFileResult> GetSignatureFile(Guid id, Guid sid)
        {
            var filePath = await signatureService.TryGetSignatureLocationAsync(sid, SignatureType.Registry, id);
            return PhysicalFile(filePath, "application/octet-stream",
                Path.GetFileName(filePath));
        }

        [HttpPost("{id}/signatures")]
        public async Task<RegistrySignatureDTO> Sign(Guid id, [FromForm] SignatureRequestDTO dto)
        {
            var signature = await signatureService.CreateAsync(id, SignatureType.Registry, mapper.Map<SignatureRequest>(dto));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.RegistrySigned,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = id.ToString(),
                Message = await auditService.GetMessageAsync<Registry>(id)
            });
            return mapper.Map<RegistrySignatureDTO>((RegistrySignature) signature);
        }

        [HttpPost("{id}/signatures/list")]
        public async Task<ResourceCollection<RegistrySignatureDTO>> SignList(
            Guid id,
            [FromForm] SignatureRequestDTO[] dto
        )
        {
            var signature = await signatureService.CreateAsync(id,
                SignatureType.Registry,
                mapper.Map<SignatureRequest[]>(dto));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.RegistrySigned,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = id.ToString(),
                Message = await auditService.GetMessageAsync<Registry>(id)
            });
            return mapper.Map<ResourceCollection<RegistrySignatureDTO>>(
                signature.OfType<RegistrySignature>().ToArray());
        }

        /// <summary>
        ///     Download zip file
        /// </summary>
        [HttpGet("{id}/files")]
        public async Task<FileStreamResult> GetFiles(Guid id)
        {
            var registryZipItems = await registryService.GetZipItemsAsync(id);
            if (!registryZipItems.Any())
            {
                throw new NotFoundException(typeof(Registry));
            }

            var signatureZipItems =
                await signatureService.TryGetSignatureZipItemsAsync(SignatureType.Registry, id);

            await using var zipper = new Zipper();
            var zipStream = zipper.Zip(registryZipItems.Concat(signatureZipItems));
            return File(zipStream, "application/octet-stream");
        }

        [HttpPost("send-email-notification")]
        public async Task<NoContentResult> SendActivation([FromBody] RegistryEmailNotificationDTO model)
        {
            var registry = await registryService.GetDetailedAsync(model.Id);
            var company = await unitOfWork.GetOrFailAsync<Entities.CompanyAggregates.Company, Guid>(model.CompanyId);
            await mailer.SendRegistryEmailAsync(registry, company, model.Type, model.ReturnUrl);
            return NoContent();
        }
    }
}