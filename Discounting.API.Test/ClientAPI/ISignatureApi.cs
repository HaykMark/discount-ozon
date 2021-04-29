using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Signature;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface ISignatureApi
    {
        [Post("/api/signatures/verify")]
        Task<SignatureInfoDTO> VerifySignature(SignatureVerificationRequestDTO dto);
    }
}