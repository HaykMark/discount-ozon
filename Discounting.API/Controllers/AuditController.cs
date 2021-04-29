using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities.Auditing;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers
{
    [ApiVersion("1.0")]
    [Route(Routes.Audit)]
    [Zone(Zones.Audit)]
    public class AuditController : BaseController
    {
        private readonly IAuditService auditService;
        private readonly IMapper mapper;

        public AuditController(IAuditService auditService, IFirewall firewall, IMapper mapper)
            : base(firewall)
        {
            this.auditService = auditService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ResourceCollection<AuditDTO>> Get(AuditFilter filter)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            var (audits, count) = await auditService.GetAsync(filter);
            return new ResourceCollection<AuditDTO>(mapper.Map<ResourceCollection<AuditDTO>>(audits), count);
        }
    }
}