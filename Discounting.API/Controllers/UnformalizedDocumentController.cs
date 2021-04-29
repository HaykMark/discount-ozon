using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Email;
using Discounting.API.Common.ViewModels.EmailNotification;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.API.Common.ViewModels.UnformalizedDocument;
using Discounting.Common.AccessControl;
using Discounting.Common.Exceptions;
using Discounting.Common.Response;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Helpers;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Models;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers
{
    [ApiVersion("1.0")]
    [Route(Routes.UnformalizedDocuments)]
    [Zone(Zones.UnformalizedDocuments)]
    public class UnformalizedDocumentController : BaseController
    {
        private readonly IMailer mailer;
        private readonly IMapper mapper;
        private readonly ISignatureService signatureService;
        private readonly IUnformalizedDocumentService unformalizedDocumentService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUploadService uploadService;
        private readonly IAuditService auditService;

        public UnformalizedDocumentController(
            IUnformalizedDocumentService unformalizedDocumentService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IFirewall firewall,
            ISignatureService signatureService,
            IUploadService uploadService,
            IMailer mailer,
            IAuditService auditService
        ) : base(firewall)
        {
            this.unformalizedDocumentService = unformalizedDocumentService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.signatureService = signatureService;
            this.uploadService = uploadService;
            this.mailer = mailer;
            this.auditService = auditService;
        }

        [HttpGet("{id}")]
        public async Task<UnformalizedDocumentDTO> Get(Guid id)
        {
            return mapper.Map<UnformalizedDocumentDTO>(await unformalizedDocumentService.GetAsync(id));
        }

        [HttpGet]
        public async Task<ResourceCollection<UnformalizedDocumentDTO>> Get(
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            DateTime? creationDateFrom = null,
            DateTime? creationDateTo = null,
            Guid? receiverId = null
        )
        {
            var (unformalizedDocuments, count) =
                await unformalizedDocumentService.GetListAsync(
                    offset,
                    limit,
                    status,
                    creationDateFrom,
                    creationDateTo,
                    receiverId
                );
            return new ResourceCollection<UnformalizedDocumentDTO>(
                mapper.Map<ResourceCollection<UnformalizedDocumentDTO>>(unformalizedDocuments), count);
        }

        [HttpGet("sent")]
        public async Task<ResourceCollection<UnformalizedDocumentDTO>> GetSent(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? sentDateFrom = null,
            DateTime? sentDateTo = null,
            Guid? receiverId = null
        )
        {
            var (unformalizedDocuments, count) = await unformalizedDocumentService.GetSentAsync(
                offset,
                limit,
                status,
                sentDateFrom,
                sentDateTo,
                receiverId
            );
            return new ResourceCollection<UnformalizedDocumentDTO>(
                mapper.Map<ResourceCollection<UnformalizedDocumentDTO>>(unformalizedDocuments), count);
        }

        [HttpGet("received")]
        public async Task<ResourceCollection<UnformalizedDocumentDTO>> GetReceived(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            UnformalizedDocumentStatus status = UnformalizedDocumentStatus.None,
            DateTime? receivedDateFrom = null,
            DateTime? receivedDateTo = null,
            Guid? senderId = null
        )
        {
            var (unformalizedDocuments, count) = await unformalizedDocumentService.GetReceivedAsync(
                offset,
                limit,
                status,
                receivedDateFrom,
                receivedDateTo,
                senderId
            );
            return new ResourceCollection<UnformalizedDocumentDTO>(
                mapper.Map<ResourceCollection<UnformalizedDocumentDTO>>(unformalizedDocuments), count);
        }

        [HttpPost("decline")]
        public async Task<UnformalizedDocumentDTO> Decline([FromBody] UnformalizedDocumentDeclineDTO dto)
        {
            var entity = await unitOfWork.GetOrFailAsync<UnformalizedDocument, Guid>(dto.Id);
            var unformalizedDocument = await unformalizedDocumentService.DeclineAsync(mapper.Map(dto, entity));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.UFDocumentDeclined,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = entity.Id.ToString(),
                Message = await auditService.GetMessageAsync<UnformalizedDocument>(unformalizedDocument.Id)
            });
            return mapper.Map<UnformalizedDocumentDTO>(unformalizedDocument);
        }

        [HttpPost]
        public async Task<UnformalizedDocumentDTO> Post([FromBody] UnformalizedDocumentDTO dto)
        {
            var unformalizedDocument =
                await unformalizedDocumentService.CreateAsync(mapper.Map<UnformalizedDocument>(dto));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.UFDocumentCreated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = unformalizedDocument.Id.ToString(),
                Message = await auditService.GetMessageAsync<UnformalizedDocument>(unformalizedDocument.Id)
            });
            return mapper.Map<UnformalizedDocumentDTO>(unformalizedDocument);
        }

        [HttpPut("{id}")]
        public async Task<UnformalizedDocumentDTO> Put(Guid id, [FromBody] UnformalizedDocumentDTO dto)
        {
            return mapper.Map<UnformalizedDocumentDTO>(
                await unformalizedDocumentService.UpdateAsync(mapper.Map<UnformalizedDocument>(dto)));
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await unformalizedDocumentService.RemoveAsync(id);
            return NoContent();
        }

        //Signatures
        [HttpGet(Routes.WithSignatures)]
        public async Task<ResourceCollection<UnformalizedDocumentSignatureDTO>> GetSignatures(Guid id)
        {
            var entities = (await signatureService.TryGetAsync(SignatureType.UnformalizedDocument, id))
                .OfType<UnformalizedDocumentSignature>()
                .ToArray();
            return new ResourceCollection<UnformalizedDocumentSignatureDTO>(
                mapper.Map<ResourceCollection<UnformalizedDocumentSignatureDTO>>(entities));
        }

        [HttpGet(Routes.SignatureFileSubRoute)]
        public async Task<PhysicalFileResult> GetSignatureFile(Guid id, Guid sid)
        {
            var filePath =
                await signatureService.TryGetSignatureLocationAsync(sid, SignatureType.UnformalizedDocument, id);
            return PhysicalFile(filePath, "application/octet-stream", Path.GetFileName(filePath));
        }

        [HttpGet(Routes.SignaturesFileSubRoute)]
        public async Task<FileStreamResult> GetSignaturesZip(Guid id)
        {
            var signatureZipItems =
                await signatureService.TryGetSignatureZipItemsAsync(SignatureType.UnformalizedDocument, id);
            if (!signatureZipItems.Any())
            {
                throw new NotFoundException(typeof(Signature));
            }

            await using var zipper = new Zipper();
            var zipStream = zipper.Zip(signatureZipItems);
            return File(zipStream, "application/octet-stream");
        }

        [HttpPost(Routes.WithSignatures)]
        public async Task<UnformalizedDocumentSignatureDTO> Sign(Guid id, [FromForm] SignatureRequestDTO dto)
        {
            var signature =
                await signatureService.CreateAsync(id, SignatureType.UnformalizedDocument,
                    mapper.Map<SignatureRequest>(dto));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.UFDocumentSigned,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = id.ToString(),
                Message = await auditService.GetMessageAsync<UnformalizedDocument>(id)
            });
            return mapper.Map<UnformalizedDocumentSignatureDTO>((UnformalizedDocumentSignature) signature);
        }

        [HttpPost("{id}/signatures/list")]
        public async Task<ResourceCollection<UnformalizedDocumentSignatureDTO>> SignList(Guid id,
            [FromForm] SignatureRequestDTO[] dtos)
        {
            var signature = await signatureService.CreateAsync(id,
                SignatureType.UnformalizedDocument,
                mapper.Map<SignatureRequest[]>(dtos));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.UFDocumentSigned,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = id.ToString(),
                Message = await auditService.GetMessageAsync<UnformalizedDocument>(id)
            });
            return mapper.Map<ResourceCollection<UnformalizedDocumentSignatureDTO>>(
                signature.OfType<UnformalizedDocumentSignature>().ToArray());
        }

        [HttpGet("{id}/files")]
        public async Task<FileStreamResult> GetSignaturesAndUploadsZip(Guid id)
        {
            var signatureZipItems =
                await signatureService.TryGetSignatureZipItemsAsync(SignatureType.UnformalizedDocument, id);
            var uploadZipItems = await uploadService.GetZipItemsAsync(UploadProvider.UnformalizedDocument, id);
            var allZipItems = signatureZipItems.Concat(uploadZipItems);
            var zipItems = allZipItems as ZipItem[] ?? allZipItems.ToArray();
            if (!zipItems.Any())
            {
                throw new NotFoundException();
            }

            await using var zipper = new Zipper();
            var zipStream = zipper.Zip(zipItems);
            return File(zipStream, "application/octet-stream");
        }

        [HttpPost("send-email-notification")]
        public async Task<NoContentResult> SendActivation([FromBody] UnformalizedDocumentEmailNotificationDTO model)
        {
            var document = await unformalizedDocumentService.GetDetailedAsync(model.Id);
            var company = await unitOfWork.GetOrFailAsync<Entities.CompanyAggregates.Company, Guid>(model.CompanyId);
            await mailer.SendUnformalizedDocumentEmailAsync(document, company, model.Type, model.ReturnUrl);
            return NoContent();
        }
    }
}