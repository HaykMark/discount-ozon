using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Validators
{
    public interface ISignatureValidator
    {
        Task ValidateAccessPermissionAsync(SignatureType type, Guid sourceId, User currentUser);
        Task ValidateAsync(SignatureType type, Guid sourceId, User currentUser);
    }

    public class SignatureValidator : ISignatureValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public SignatureValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ValidateAccessPermissionAsync(SignatureType type, Guid sourceId, User currentUser)
        {
            if (currentUser.IsSuperAdmin)
            {
                return;
            }

            switch (type)
            {
                case SignatureType.Registry:
                    var registry =
                        await unitOfWork.GetOrFailAsync(sourceId,
                            unitOfWork.Set<Registry>().Include(r => r.Contract));
                    if (!currentUser.IsSuperAdmin &&
                        registry.Contract.SellerId != currentUser.CompanyId &&
                        registry.Contract.BuyerId != currentUser.CompanyId &&
                        (!registry.BankId.HasValue || registry.BankId.Value != currentUser.CompanyId)
                    )
                    {
                        throw new ForbiddenException();
                    }

                    break;
                case SignatureType.CompanyRegulation when
                    !await unitOfWork.Set<CompanyRegulation>()
                        .AnyAsync(c => c.Id == sourceId &&
                                       c.Company.Id == currentUser.CompanyId):
                {
                    throw new ForbiddenException();
                }

                case SignatureType.UnformalizedDocument when
                    !await unitOfWork.Set<UnformalizedDocument>()
                        .AnyAsync(d => d.Id == sourceId &&
                                       (d.SenderId == currentUser.CompanyId ||
                                        (d.IsSent && d.Receivers.Any(r => r.ReceiverId == currentUser.CompanyId)))):
                {
                    throw new ForbiddenException();
                }

                case SignatureType.UserRegulation when
                    !await unitOfWork.Set<UserRegulation>()
                        .AnyAsync(u => u.Id == sourceId &&
                                       u.User.CompanyId == currentUser.CompanyId):
                {
                    throw new ForbiddenException();
                }

                case SignatureType.Upload when
                    !await unitOfWork.Set<Upload>()
                        .AnyAsync(u => u.Id == sourceId &&
                                       u.User.CompanyId == currentUser.CompanyId):
                {
                    throw new ForbiddenException();
                }
            }
        }

        public async Task ValidateAsync(SignatureType type, Guid sourceId, User currentUser)
        {
            if (!currentUser.CanSign)
            {
                throw new ForbiddenException("no-permission-to-sign",
                    "Current user has no permission to sign");
            }

            switch (type)
            {
                case SignatureType.Registry:
                    if (await unitOfWork.Set<RegistrySignature>().AnyAsync(r =>
                        r.RegistryId == sourceId && r.Signer.CompanyId == currentUser.CompanyId))
                    {
                        throw new ForbiddenException("registry-is-already-signed-by-this-company",
                            "You cannot sign twice with the same company");
                    }

                    if (!await unitOfWork.Set<Registry>()
                        .AnyAsync(r => r.Id == sourceId &&
                                       (r.Contract.SellerId == currentUser.CompanyId &&
                                        (r.SignStatus == RegistrySignStatus.NotSigned ||
                                         r.SignStatus == RegistrySignStatus.SignedByBuyer)) ||
                                       (r.Contract.BuyerId == currentUser.CompanyId &&
                                        (r.SignStatus == RegistrySignStatus.NotSigned ||
                                         r.SignStatus == RegistrySignStatus.SignedBySeller)) ||
                                       (r.BankId.HasValue &&
                                        r.BankId.Value == currentUser.CompanyId &&
                                        r.SignStatus == RegistrySignStatus.SignedBySellerBuyer)))
                    {
                        throw new NotFoundException(typeof(Registry));
                    }

                    break;
                case SignatureType.CompanyRegulation:
                    if (await unitOfWork.Set<CompanyRegulationSignature>().AnyAsync(r =>
                        r.CompanyRegulationId == sourceId && r.Signer.CompanyId == currentUser.CompanyId))
                    {
                        throw new ForbiddenException("regulation-is-already-signed-by-this-company",
                            "You cannot sign twice with the same company");
                    }

                    if (!await unitOfWork.Set<CompanyRegulation>()
                        .AnyAsync(c => c.Id == sourceId &&
                                       c.CompanyId == currentUser.CompanyId))
                    {
                        throw new NotFoundException(typeof(CompanyRegulation));
                    }

                    break;
                case SignatureType.UnformalizedDocument:
                    if (await unitOfWork.Set<UnformalizedDocumentSignature>()
                        .AnyAsync(r =>
                            r.UnformalizedDocumentId == sourceId &&
                            r.Signer.CompanyId == currentUser.CompanyId))
                    {
                        throw new ForbiddenException("unformalized-document-is-already-signed-by-this-company",
                            "You cannot sign twice with the same company");
                    }

                    if (!await unitOfWork.Set<UnformalizedDocument>()
                        .AnyAsync(d => d.Id == sourceId &&
                                       d.Status != UnformalizedDocumentStatus.SignedByAll &&
                                       d.Status != UnformalizedDocumentStatus.Declined &&
                                       (d.SenderId == currentUser.CompanyId ||
                                        (d.IsSent &&
                                         d.Receivers.Any(r => r.ReceiverId == currentUser.CompanyId &&
                                                              r.NeedSignature)))))
                    {
                        throw new NotFoundException(typeof(CompanyRegulation));
                    }

                    break;
                case SignatureType.UserRegulation:
                    if (await unitOfWork.Set<UserRegulationSignature>()
                        .AnyAsync(r =>
                            r.UserRegulationId == sourceId &&
                            r.SignerId == currentUser.Id))
                    {
                        throw new ForbiddenException("regulation-is-already-signed-by-this-user",
                            "You cannot sign twice with the same user");
                    }

                    if (!await unitOfWork.Set<UserRegulation>()
                        .AnyAsync(c => c.Id == sourceId &&
                                       c.User.CompanyId == currentUser.CompanyId))
                    {
                        throw new NotFoundException(typeof(CompanyRegulation));
                    }

                    break;
                case SignatureType.Upload:
                    if (await unitOfWork.Set<UploadSignature>()
                        .AnyAsync(r =>
                            r.UploadId == sourceId &&
                            r.SignerId == currentUser.Id))
                    {
                        throw new ForbiddenException("upload-is-already-signed-by-this-user",
                            "You cannot sign twice with the same user");
                    }

                    break;
            }
        }
    }
}