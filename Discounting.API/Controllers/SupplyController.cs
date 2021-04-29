using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Services;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Discounting.API.Controllers
{
    [ApiVersion("1.0")]
    [Route(Routes.Supplies)]
    [Zone(Zones.Supplies)]
    public class SupplyController : BaseController
    {
        private readonly IMapper mapper;
        private readonly ISupplyExcelParsingService supplyExcelParsingService;
        private readonly ISupplyService supplyService;
        private readonly IAuditService auditService;
        private readonly IOptions<MvcNewtonsoftJsonOptions> jsonOptions;

        public SupplyController(
            IMapper mapper,
            ISupplyService supplyService,
            IFirewall firewall,
            ISupplyExcelParsingService supplyExcelParsingService,
            IAuditService auditService,
            IOptions<MvcNewtonsoftJsonOptions> jsonOptions
        )
            : base(firewall)
        {
            this.mapper = mapper;
            this.supplyService = supplyService;
            this.supplyExcelParsingService = supplyExcelParsingService;
            this.auditService = auditService;
            this.jsonOptions = jsonOptions;
        }

        [HttpGet("in-process")]
        public async Task<ResourceCollection<SupplyDTO>> GetInProcess(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(Operations.Read, Zones.SuppliesInProcess);
            var (supplies, count) = await supplyService.GetInProcess(offset, limit);
            return new ResourceCollection<SupplyDTO>(mapper.Map<ResourceCollection<SupplyDTO>>(supplies), count);
        }

        [HttpGet("in-finance")]
        public async Task<ResourceCollection<SupplyDTO>> GetInFinance(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(Operations.Read, Zones.SuppliesInFinance);
            var (supplies, count) = await supplyService.GetInFinance(offset, limit);
            return new ResourceCollection<SupplyDTO>(mapper.Map<ResourceCollection<SupplyDTO>>(supplies), count);
        }

        [HttpGet("not-available")]
        public async Task<ResourceCollection<SupplyDTO>> GetNotAvailable(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(Operations.Read, Zones.SuppliesNotAvailable);
            var (supplies, count) = await supplyService.GetNotAvailable(offset, limit);
            return new ResourceCollection<SupplyDTO>(mapper.Map<ResourceCollection<SupplyDTO>>(supplies), count);
        }

        [HttpGet("{id}")]
        public async Task<SupplyDTO> Get(Guid id)
        {
            return mapper.Map<SupplyDTO>(await supplyService.Get(id));
        }

        [HttpGet("{id}/discount")]
        public async Task<SupplyDiscountDTO> GetDiscount(Guid id)
        {
            return mapper.Map<SupplyDiscountDTO>(await supplyService.GetDiscount(id));
        }

        [HttpPost]
        public async Task<SupplyResponseDTO> Post([FromBody] List<SupplyDTO> model)
        {
            var (supplies, validationErrors) = await supplyService.CreateAsync(
                mapper.Map<List<Supply>>(model),
                SupplyProvider.FromApi
            );
            var supplyDtos = mapper.Map<List<SupplyDTO>>(supplies);
            if (supplies.Any())
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.SuppliesAdded,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    Message = "Номера поставок: " + string.Join(", ", supplyDtos.Select(s => s.Number))
                });
            }

            if (validationErrors.Any())
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.SuppliesAdded,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    Message = JsonConvert.SerializeObject(validationErrors, jsonOptions.Value.SerializerSettings)
                });
            }

            return new SupplyResponseDTO
            {
                Supplies = supplyDtos,
                Errors = validationErrors
            };
        }

        [HttpPost("verify-seller-manually")]
        public async Task<SupplyResponseDTO> VerifySeller([FromBody] SupplyVerificationRequestDTO model)
        {
            var (supplies, validationErrors) = await supplyService.VerifySellerManualAsync(
                model.SupplyIds,
                model.BankId,
                model.FactoringAgreementId
            );
            var supplyDtos = mapper.Map<List<SupplyDTO>>(supplies);
            if (supplies.Any())
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.SuppliesVerifiedSeller,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    Message = "Номера поставок: " + string.Join(", ", supplyDtos.Select(s => s.Number))
                });
            }

            if (validationErrors.Any())
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.SuppliesVerifiedSeller,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Failed,
                    IpAddress = GetIpAddress(),
                    Message = JsonConvert.SerializeObject(validationErrors, jsonOptions.Value.SerializerSettings)
                });
            }

            return new SupplyResponseDTO
            {
                Supplies = supplyDtos,
                Errors = validationErrors
            };
        }

        [HttpPost("verify-buyer-manually")]
        public async Task<SupplyResponseDTO> VerifyBuyer([FromBody] Guid[] supplyIds)
        {
            var (supplies, validationErrors) = await supplyService.VerifyBuyerManualAsync(supplyIds);
            var supplyDtos = mapper.Map<List<SupplyDTO>>(supplies);
            if (supplies.Any())
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.SuppliesVerifiedBuyer,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    Message = "Номера поставок: " + string.Join(", ", supplyDtos.Select(s => s.Number))
                });
            }

            if (validationErrors.Any())
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.SuppliesVerifiedBuyer,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Failed,
                    IpAddress = GetIpAddress(),
                    Message = JsonConvert.SerializeObject(validationErrors, jsonOptions.Value.SerializerSettings)
                });
            }

            return new SupplyResponseDTO
            {
                Supplies = supplyDtos,
                Errors = validationErrors
            };
        }

        // [HttpPost("verify-automatically")]
        // public async Task<SupplyResponseDTO> Verify([FromBody] Guid[] supplyIds)
        // {
        //     var (supplies, validationErrors) = await supplyService.VerifyAutomaticallyAsync(supplyIds);
        //     var supplyDtos = mapper.Map<List<SupplyDTO>>(supplies);
        //     return new SupplyResponseDTO
        //     {
        //         Supplies = supplyDtos,
        //         Errors = validationErrors
        //     };
        // }

        [HttpPost("excel")]
        public async Task<SupplyResponseDTO> CreateFromExcelAsync([FromForm] IFormFile file)
        {
            var supplyResponseDto = await supplyExcelParsingService.TryParseExcelAsync(file);
            if (supplyResponseDto.Supplies.Any())
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.SuppliesAdded,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    Message = "Номера поставок: " + string.Join(", ", supplyResponseDto.Supplies.Select(s => s.Number))
                });
            }

            if (supplyResponseDto.FailedCount > 0)
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.SuppliesAdded,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Failed,
                    IpAddress = GetIpAddress(),
                    Message = supplyResponseDto.FailedCount + " не загруженных поставок"
                });
            }

            return supplyResponseDto;
        }
    }
}