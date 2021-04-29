using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Regulations;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Data.Context;
using Discounting.Entities.Regulations;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Regulations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Regulations
{
    [ApiVersion("1.0")]
    [Route(Routes.Regulations)]
    [Zone(Zones.Regulations)]
    public class RegulationController : BaseController
    {
        private readonly IMapper mapper;
        private readonly IRegulationService regulationService;
        private readonly IUnitOfWork unitOfWork;

        public RegulationController(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IRegulationService regulationService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.regulationService = regulationService;
        }

        [HttpGet]
        [AllowAnonymous]
        [DisableControllerZone]
        public async Task<ResourceCollection<RegulationDTO>> Get(RegulationType? type = null)
        {
            return mapper.Map<ResourceCollection<RegulationDTO>>(await regulationService.GetAllAsync(type));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [DisableControllerZone]
        public async Task<RegulationDTO> Get(Guid id)
        {
            return mapper.Map<RegulationDTO>(await regulationService.Get(id));
        }

        [HttpPost]
        public async Task<RegulationDTO> Post([FromBody] RegulationDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            return mapper.Map<RegulationDTO>(await regulationService.CreateAsync(mapper.Map<Regulation>(model)));
        }

        [HttpPut("{id}")]
        public async Task<RegulationDTO> Put(Guid id, [FromBody] RegulationDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            return mapper.Map<RegulationDTO>(await regulationService.UpdateAsync(mapper.Map<Regulation>(model)));
        }

        /// <summary>
        ///     Loads the actual file
        /// </summary>
        [HttpGet("{id}/file")]
        [AllowAnonymous]
        [DisableControllerZone]
        public async Task<PhysicalFileResult> GetFile(Guid id)
        {
            var (filePath, contentType, fileName) = await regulationService.GetRegulationFilePath(id);
            return PhysicalFile(filePath, contentType, fileName);
        }
    }
}