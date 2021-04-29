using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Templates;
using Discounting.Common.Response;
using Discounting.Entities.Templates;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Templates;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Templates
{
    [ApiVersion("1.0")]
    [Route(Routes.Templates)]
    public class TemplateController : BaseController
    {
        private readonly IMapper mapper;
        private readonly IUploadPathProviderService pathProviderService;
        private readonly ITemplateService templateService;

        public TemplateController(
            IMapper mapper,
            IFirewall firewall,
            ITemplateService templateService,
            IUploadPathProviderService pathProviderService
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.templateService = templateService;
            this.pathProviderService = pathProviderService;
        }

        [HttpGet]
        public async Task<ResourceCollection<TemplateDTO>> Get(Guid? companyId, TemplateType? type = null)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            return mapper.Map<ResourceCollection<TemplateDTO>>(await templateService.GetAllAsync(companyId, type));
        }

        [HttpGet("{id}")]
        public async Task<TemplateDTO> Get(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            return mapper.Map<TemplateDTO>(await templateService.Get(id));
        }

        [HttpPost]
        public async Task<TemplateDTO> Post([FromForm] TemplateRequestDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            return mapper.Map<TemplateDTO>(await templateService.CreateAsync(model.CompanyId, model.Type, model.File));
        }

        [HttpPut("{id}")]
        public async Task<TemplateDTO> Put(Guid id, [FromForm] TemplateRequestDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            return mapper.Map<TemplateDTO>(
                await templateService.UpdateAsync(model.Id, model.CompanyId, model.Type, model.File));
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            await templateService.RemoveAsync(id);
            return new NoContentResult();
        }

        /// <summary>
        ///     Loads the actual file
        /// </summary>
        [HttpGet(Routes.FileSubRoute)]
        public async Task<PhysicalFileResult> GetFile(Guid id)
        {
            var template = await templateService.Get(id);
            var filePath = pathProviderService.GetTemplatePath(template.Id, template.ContentType, template.Type);
            return PhysicalFile(filePath, template.ContentType, template.Name);
        }

        [HttpGet("lookups")]
        public async Task<ResourceCollection<TemplateLookupsDTO>> GetLookups(Guid? companyId, TemplateType? type = null)
        {
            return mapper.Map<ResourceCollection<TemplateLookupsDTO>>(
                await templateService.GetAllAsync(companyId, type));
        }
    }
}