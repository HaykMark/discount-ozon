using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.Common.AccessControl;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Logics;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers
{
    [ApiVersion("1.0")]
    [Route(Routes.Signatures)]
    [Zone(Zones.Signatures)]
    public class SignatureController : BaseController
    {
        private readonly SignatureVerifierService signatureVerifierService;
        private readonly IMapper mapper;
        private readonly IAuditService auditService;

        public SignatureController(SignatureVerifierService signatureVerifierService, IMapper mapper,
            IAuditService auditService)
        {
            this.signatureVerifierService = signatureVerifierService;
            this.mapper = mapper;
            this.auditService = auditService;
        }

        [HttpPost("verify")]
        public async Task<SignatureInfoDTO> VerifySignature(
            [FromBody] SignatureVerificationRequestDTO signatureVerificationRequestDTO)
        {
            var verificationResponse = await signatureVerifierService.GetVerificationCenterResponseAsync(
                signatureVerificationRequestDTO.Signature,
                signatureVerificationRequestDTO.Original);
            var incident = signatureVerificationRequestDTO.Type switch
            {
                SignatureType.Registry => IncidentType.RegistrySignatureVerification,
                SignatureType.UnformalizedDocument => IncidentType.UFDocumentSignatureVerification,
                SignatureType.CompanyRegulation => IncidentType.CompanyRegulationSignatureVerification,
                SignatureType.UserRegulation => IncidentType.UserRegulationSignatureVerification,
                _ => throw new ArgumentOutOfRangeException()
            };
            await auditService.CreateAsync(new Audit
            {
                UserId = GetUserId(),
                Incident = incident,
                IncidentDate = DateTime.UtcNow,
                IncidentResult = verificationResponse.Success 
                    ? IncidentResult.Success 
                    : IncidentResult.Failed,
                IpAddress = GetIpAddress(),
                Message = verificationResponse.Result
            });
            return mapper.Map<SignatureInfoDTO>(signatureVerifierService.GetSignatureInfo(verificationResponse));
        }
    }
}