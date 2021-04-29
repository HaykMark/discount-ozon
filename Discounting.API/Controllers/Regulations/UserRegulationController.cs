using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Filters;
using Discounting.API.Common.ViewModels.Regulations;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Entities.Regulations;
using Discounting.Helpers;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Models;
using Discounting.Logics.Regulations;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Regulations
{
    [ApiVersion("1.0")]
    [Route(Routes.UserRegulations)]
    [Zone(Zones.UserRegulations)]
    public class UserRegulationController : BaseController
    {
        private readonly IMapper mapper;
        private readonly ISignatureService signatureService;
        private readonly IUserRegulationService userRegulationService;
        private readonly IAuditService auditService;

        public UserRegulationController(
            IMapper mapper,
            IFirewall firewall,
            IUserRegulationService userRegulationService,
            ISignatureService signatureService,
            IAuditService auditService
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.userRegulationService = userRegulationService;
            this.signatureService = signatureService;
            this.auditService = auditService;
        }

        [HttpGet]
        public async Task<ResourceCollection<UserRegulationDTO>> Get(Guid? userId = null)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            return mapper.Map<ResourceCollection<UserRegulationDTO>>(
                await userRegulationService.GetAllAsync(userId));
        }

        [HttpGet("{id}")]
        public async Task<UserRegulationDTO> Get(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            return mapper.Map<UserRegulationDTO>(await userRegulationService.GetAsync(id));
        }

        [HttpPost]
        public async Task<UserRegulationDTO> Post([FromBody] UserRegulationDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            var userRegulation = await userRegulationService.CreateAsync(mapper.Map<UserRegulation>(model));
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = IncidentType.NewUserRegulationCreated,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = IncidentResult.Success,
                IpAddress = GetIpAddress(),
                SourceId = userRegulation.Id.ToString()
            });
            return mapper.Map<UserRegulationDTO>(userRegulation);
        }

        [HttpPut("{id}/profile")]
        [DisableRoutValidator]
        public async Task<UserProfileRegulationInfoDTO> Put(Guid id, [FromBody] UserProfileRegulationInfoDTO model)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            var userRegulation =
                await userRegulationService.UpdateProfileAsync(mapper.Map<UserProfileRegulationInfo>(model));
            return mapper.Map<UserProfileRegulationInfoDTO>(userRegulation);
        }

        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsAdmin);
            await userRegulationService.RemoveAsync(id);
            return new NoContentResult();
        }

        /// <summary>
        ///     Generates template for user profile regulation
        /// </summary>
        [HttpGet("{id}/profile")]
        public async Task<PhysicalFileResult> GetProfileFile(Guid id)
        {
            var filePath = await userRegulationService.GetProfileFileAsync(id);
            return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Анкета.xlsx");
        }

        [HttpGet(Routes.WithSignatures)]
        public async Task<ResourceCollection<UserRegulationSignatureDTO>> GetSignatures(Guid id)
        {
            var entities = (await signatureService.TryGetAsync(SignatureType.UserRegulation, id))
                .OfType<UserRegulationSignature>()
                .ToArray();
            return new ResourceCollection<UserRegulationSignatureDTO>(
                mapper.Map<ResourceCollection<UserRegulationSignatureDTO>>(entities));
        }

        [HttpGet(Routes.SignatureFileSubRoute)]
        public async Task<PhysicalFileResult> GetSignatureFile(Guid id, Guid sid)
        {
            var filePath =
                await signatureService.TryGetSignatureLocationAsync(sid, SignatureType.UserRegulation, id);
            return PhysicalFile(filePath, "application/octet-stream", Path.GetFileName(filePath));
        }

        [HttpGet(Routes.SignaturesFileSubRoute)]
        public async Task<FileStreamResult> GetSignaturesZip(Guid id)
        {
            var signatureZipItems =
                await signatureService.TryGetSignatureZipItemsAsync(SignatureType.UserRegulation, id);
            await using var zipper = new Zipper();
            var zipStream = zipper.Zip(signatureZipItems);
            return File(zipStream, "application/octet-stream");
        }


        [HttpPost("signatures/list")]
        public async Task<ResourceCollection<UserRegulationSignatureDTO>> SignList(
            [FromForm] SignatureRequestDTO[] dtos)
        {
            var signature = await signatureService.CreateAsync(Guid.Empty,
                SignatureType.UserRegulation,
                mapper.Map<SignatureRequest[]>(dtos));
            foreach (var signatureRequestDto in dtos)
            {
                await auditService.CreateAsync(new Audit
                {
                    UserId = GetUserId(),
                    Incident = IncidentType.NewUserRegulationSigned,
                    IncidentDate = DateTime.UtcNow,
                    IncidentResult = IncidentResult.Success,
                    IpAddress = GetIpAddress(),
                    SourceId = signatureRequestDto.SourceId.ToString()
                });
            }

            return mapper.Map<ResourceCollection<UserRegulationSignatureDTO>>(
                signature.OfType<UserRegulationSignature>().ToArray());
        }

        [HttpGet("files/{userId}")]
        public async Task<FileStreamResult> GetSignaturesAndUploadsZip(Guid userId)
        {
            var allZipItems = await userRegulationService.GetZipItemsAsync(userId);
            await using var zipper = new Zipper();
            var zipStream = zipper.Zip(allZipItems);
            return File(zipStream, "application/octet-stream", "Регламенты.zip");
        }
    }
}