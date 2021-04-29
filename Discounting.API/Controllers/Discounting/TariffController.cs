using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Data.Context;
using Discounting.Entities.Auditing;
using Discounting.Entities.TariffDiscounting;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.TariffDiscounting;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Discounting
{
    [ApiVersion("1.0")]
    [Route(Routes.Tariffs)]
    [Zone(Zones.Tariffs)]
    public class TariffController : BaseController
    {
        private readonly IMapper mapper;
        private readonly ITariffService tariffService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuditService auditService;

        public TariffController(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IFirewall firewall,
            ITariffService tariffService,
            IAuditService auditService
        )
            : base(firewall)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.tariffService = tariffService;
            this.auditService = auditService;
        }

        /// <summary>
        ///     This endpoint is for SuperAdmins
        ///     Normal users can use this endpoint only by passing companyId query param
        ///     If you want to get current companies Tariffs please consider using /companies/Id/Tariffs endpoint
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResourceCollection<TariffDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            Guid? companyId = null
        )
        {
            if (!companyId.HasValue)
            {
                await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            }

            var (tariffs, count) = await tariffService.GetAll(offset, limit, companyId);
            return new ResourceCollection<TariffDTO>(mapper.Map<ResourceCollection<TariffDTO>>(tariffs), count);
        }

        [HttpGet("{id}")]
        public async Task<TariffDTO> Get(Guid id)
        {
            return mapper.Map<TariffDTO>(await unitOfWork.GetOrFailAsync<Tariff, Guid>(id));
        }

        [HttpPost]
        public async Task<ResourceCollection<TariffDTO>> Post([FromBody] TariffDTO[] model)
        {
            var tariffs = await tariffService.CreateAsync(mapper.Map<Tariff[]>(model));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.TariffCreated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                Message = string.Join("; ", tariffs.Select(t => t.Id))
            });
            return mapper.Map<ResourceCollection<TariffDTO>>(tariffs);
        }

        [HttpGet("archives")]
        public async Task<ResourceCollection<TariffArchiveDTO>> GetArchives(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            Guid? companyId = null
        )
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);

            var (tariffs, count) = await tariffService.GetArchives(offset, limit, companyId);
            return new ResourceCollection<TariffArchiveDTO>(
                mapper.Map<ResourceCollection<TariffArchiveDTO>>(tariffs), count);
        }
    }
}