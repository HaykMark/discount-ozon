using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels;
using Discounting.Common.Exceptions;
using Discounting.Common.Response;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Helpers;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers
{
    /// <summary>
    ///     Api endpoint for Uploads.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(Routes.Uploads)]
    public class UploadController : BaseController
    {
        private readonly IMapper mapper;
        private readonly IUploadPathProviderService pathProviderService;
        private readonly ISignatureService signatureService;
        private readonly IUploadService uploadService;
        private readonly IAuditService auditService;

        public UploadController(
            IMapper mapper,
            IUploadService uploadService,
            IFirewall firewall,
            IUploadPathProviderService pathProviderService,
            ISignatureService signatureService,
            IAuditService auditService
        )
            : base(firewall)
        {
            this.mapper = mapper;
            this.uploadService = uploadService;
            this.pathProviderService = pathProviderService;
            this.signatureService = signatureService;
            this.auditService = auditService;
        }

        [HttpGet]
        public async Task<ResourceCollection<UploadDTO>> Get(Guid? providerId = null, UploadProvider? provider = null)
        {
            if (providerId is null && provider is null)
            {
                await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            }

            var uploads = await uploadService
                .GetAllAsync(u => (!providerId.HasValue ||
                                   u.ProviderId == providerId.Value) &&
                                  (!provider.HasValue ||
                                   u.Provider == provider.Value));
            return mapper.Map<ResourceCollection<UploadDTO>>(uploads);
        }

        [HttpGet("{id}")]
        public async Task<UploadDTO> Get(Guid id)
        {
            var upload = await uploadService.GetAsync(id);
            return mapper.Map<UploadDTO>(upload);
        }

        /// <summary>
        ///     Download the actual file
        /// </summary>
        [HttpGet(Routes.FileSubRoute)]
        public async Task<PhysicalFileResult> GetFile(Guid id)
        {
            var upload = await uploadService.GetAsync(id);
            var filePath = pathProviderService.GetUploadPath(upload.Id, upload.ContentType, upload.Provider);
            return PhysicalFile(filePath, upload.ContentType, upload.Name);
        }

        /// <summary>
        ///     Download zip file
        /// </summary>
        [HttpGet("files")]
        public async Task<FileStreamResult> GetFiles(UploadProvider provider, Guid providerId)
        {
            var zipItems = await uploadService.GetZipItemsAsync(provider, providerId);
            if (!zipItems.Any())
            {
                throw new NotFoundException(typeof(Upload));
            }

            await using var zipper = new Zipper();
            var zipStream = zipper.Zip(zipItems);
            return File(zipStream, "application/octet-stream");
        }

        // Use when posting multiple files for one provider
        // e.g. UnformalizedDocuments
        [HttpPost("files")]
        public async Task<ResourceCollection<UploadDTO>> PostFiles([FromForm] FilesUploadRequestDTO dto)
        {
            var uploads = await uploadService.UploadFilesAsync(dto.ProviderId, dto.Provider, dto.Files);
            await AddAuditDataAsync(dto.ProviderId, dto.Provider);
            return mapper.Map<ResourceCollection<UploadDTO>>(uploads);
        }

        [HttpPost("file")]
        public async Task<UploadDTO> PostFile([FromForm] FileUploadRequestDTO dto)
        {
            if (dto.Provider == UploadProvider.Regulation)
                await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            var uploads = await uploadService.UploadFileAsync(dto.ProviderId, dto.Provider, dto.File);
            await AddAuditDataAsync(dto.ProviderId, dto.Provider);
            return mapper.Map<UploadDTO>(uploads);
        }

        [HttpPut("{id}/file")]
        public async Task<UploadDTO> UpdateFile(Guid id, [FromForm] FileUploadRequestDTO dto)
        {
            if (dto.Provider == UploadProvider.Regulation)
                await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            var upload = await uploadService.UpdateFileAsync(id, dto.ProviderId, dto.Provider, dto.File);
            switch (upload.Provider)
            {
                case UploadProvider.UnformalizedDocument:
                    await signatureService.RemoveIfAnyAsync(SignatureType.UnformalizedDocument, upload.ProviderId);
                    break;
                case UploadProvider.UserRegulation:
                    await signatureService.RemoveIfAnyAsync(SignatureType.UserRegulation, upload.ProviderId);
                    break;
            }

            return mapper.Map<UploadDTO>(upload);
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> RemoveAsync(Guid id)
        {
            var upload = await uploadService.RemoveAsync(id);
            if (upload.Provider == UploadProvider.UserRegulation)
                await signatureService.RemoveIfAnyAsync(SignatureType.UserRegulation, id);
            return NoContent();
        }

        private async Task AddAuditDataAsync(Guid sourceId, UploadProvider provider)
        {
            IncidentType type;
            switch (provider)
            {
                case UploadProvider.Regulation:
                    type = IncidentType.CompanyRegulationUploaded;
                    break;
                case UploadProvider.UserRegulation:
                    type = IncidentType.NewUserRegulationUploaded;
                    break;
                default:
                    return;
            }

            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = type,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = sourceId.ToString()
            });
        }
    }
}