using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation.Errors;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;
using Discounting.Entities.TariffDiscounting;
using Discounting.Helpers;
using Discounting.Logics.Account;
using Discounting.Logics.Models;
using Discounting.Logics.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics
{
    public interface ISignatureService
    {
        Task<ZipItem[]> TryGetSignatureZipItemsAsync(SignatureType type, Guid sourceId);
        ZipItem[] TryGetSignatureZipItems(Signature[] signatures, SignatureType type);
        Task<string> TryGetSignatureLocationAsync(Guid id, SignatureType type, Guid sourceId);
        Task<Signature[]> TryGetAsync(SignatureType sourceType, Guid sourceId);
        Task<List<Signature>> CreateAsync(Guid sourceId, SignatureType type, SignatureRequest[] signatureRequests);
        Task<Signature> CreateAsync(Guid sourceId, SignatureType type, SignatureRequest signatureRequest);
        Task RemoveIfAnyAsync(SignatureType type, Guid sourceId);
    }

    public class SignatureService : ISignatureService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;
        private readonly ISignatureValidator signatureValidator;
        private readonly IUploadService uploadService;
        private readonly IUploadPathProviderService pathProviderService;
        private readonly SignatureVerifierService signatureVerifierService;

        public SignatureService(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            ISignatureValidator signatureValidator,
            IUploadService uploadService,
            IUploadPathProviderService pathProviderService,
            SignatureVerifierService signatureVerifierService
        )
        {
            this.unitOfWork = unitOfWork;
            this.sessionService = sessionService;
            this.signatureValidator = signatureValidator;
            this.uploadService = uploadService;
            this.pathProviderService = pathProviderService;
            this.signatureVerifierService = signatureVerifierService;
        }

        public async Task<string> TryGetSignatureLocationAsync(Guid id, SignatureType type, Guid sourceId)
        {
            await ValidateSignatureAccessPermissionAsync(type, sourceId);
            var signature = await unitOfWork.GetOrFailAsync<Signature, Guid>(id);
            return pathProviderService.GetSignaturePath(signature.Name, type);
        }

        public async Task<ZipItem[]> TryGetSignatureZipItemsAsync(SignatureType type, Guid sourceId)
        {
            var signatures = await TryGetAsync(type, sourceId);
            return TryGetSignatureZipItems(signatures, type);
        }

        public ZipItem[] TryGetSignatureZipItems(Signature[] signatures, SignatureType type)
        {
            return signatures
                .Select(signature =>
                    new ZipItem
                    {
                        Path = pathProviderService.GetSignaturePath(signature.Name, type),
                        Name = $"{signature.Name.Replace(' ', '_')}.sgn"
                    })
                .ToArray();
        }

        public async Task<Signature[]> TryGetAsync(SignatureType type, Guid sourceId)
        {
            await ValidateSignatureAccessPermissionAsync(type, sourceId);
            return type switch
            {
                SignatureType.Registry =>
                    await unitOfWork.Set<RegistrySignature>()
                        .Include(u => u.SignatureInfo)
                        .Include(u => u.Signer)
                        .ThenInclude(c => c.Company)
                        .Where(r => r.RegistryId == sourceId)
                        .ToArrayAsync(),
                SignatureType.UnformalizedDocument =>
                    await unitOfWork.Set<UnformalizedDocumentSignature>()
                        .Include(u => u.SignatureInfo)
                        .Include(u => u.Signer)
                        .ThenInclude(u => u.Company)
                        .Where(r => r.UnformalizedDocumentId == sourceId)
                        .ToArrayAsync(),
                SignatureType.Upload =>
                    await unitOfWork.Set<UploadSignature>()
                        .Include(u => u.SignatureInfo)
                        .Where(r => r.UploadId == sourceId)
                        .ToArrayAsync(),
                SignatureType.CompanyRegulation =>
                    await unitOfWork.Set<CompanyRegulationSignature>()
                        .Include(u => u.SignatureInfo)
                        .Include(u => u.Signer)
                        .ThenInclude(u => u.Company)
                        .Where(r => r.CompanyRegulationId == sourceId)
                        .ToArrayAsync(),
                SignatureType.UserRegulation =>
                    await unitOfWork.Set<UserRegulationSignature>()
                        .Include(u => u.SignatureInfo)
                        .Include(u => u.Signer)
                        .ThenInclude(u => u.Company)
                        .Where(s => s.UserRegulationId == sourceId)
                        .ToArrayAsync(),
                _ => throw new NotFoundException()
            };
        }

        public async Task<List<Signature>> CreateAsync(
            Guid sourceId,
            SignatureType type,
            SignatureRequest[] signatureRequests
        )
        {
            var userId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.Set<User>()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (type == SignatureType.UserRegulation)
            {
                foreach (var signatureRequest in signatureRequests)
                {
                    await signatureValidator.ValidateAsync(type, signatureRequest.SourceId, currentUser);
                }
            }
            else
            {
                await signatureValidator.ValidateAsync(type, sourceId, currentUser);
            }

            var signatures = new List<Signature>();
            foreach (var signatureRequest in signatureRequests)
            {
                // var signatureInfo = await TryGetSignatureInfo(
                //     signatureRequest.SourceId,
                //     type,
                //     signatureRequest.OriginalName,
                //     signatureRequest.File,
                //     currentUser
                // );
                var newSignature = await uploadService.UploadSignatureAsync(
                    signatureRequest.SourceId,
                    type,
                    signatureRequest.File,
                    $"{signatureRequest.FileName}_{currentUser.Company.TIN}.sgn"
                );
                // signatureInfo.SignatureId = newSignature.Id;
                // newSignature.SignatureInfo = signatureInfo;
                signatures.Add(newSignature);
            }

            try
            {
                switch (type)
                {
                    case SignatureType.Registry:
                        signatures.ForEach(signature =>
                        {
                            var registrySignature = signature as RegistrySignature;
                            unitOfWork.Set<RegistrySignature>().Add(registrySignature);
                        });
                        await UpdateRegistryAsync(sourceId, currentUser);
                        break;
                    case SignatureType.CompanyRegulation:
                        signatures.ForEach(signature =>
                        {
                            var companyRegulationSignature = signature as CompanyRegulationSignature;
                            unitOfWork.Set<CompanyRegulationSignature>().Add(companyRegulationSignature);
                        });
                        break;
                    case SignatureType.UnformalizedDocument:
                        signatures.ForEach(signature =>
                        {
                            var unformalizedDocumentSignature = signature as UnformalizedDocumentSignature;
                            unitOfWork.Set<UnformalizedDocumentSignature>().Add(unformalizedDocumentSignature);
                        });
                        await UpdateUnformalizedDocumentAsync(sourceId, currentUser);
                        break;
                    case SignatureType.UserRegulation:
                        signatures.ForEach(signature =>
                        {
                            var userProfileRegulationSignature = signature as UserRegulationSignature;
                            unitOfWork.Set<UserRegulationSignature>().Add(userProfileRegulationSignature);
                        });
                        break;
                    case SignatureType.Upload:
                        signatures.ForEach(signature =>
                        {
                            var uploadSignature = signature as UploadSignature;
                            unitOfWork.Set<UploadSignature>().Add(uploadSignature);
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch
            {
                foreach (var filePath in signatures
                    .Select(signature => pathProviderService.GetSignaturePath(signature.Name, signature.Type))
                    .Where(File.Exists))
                {
                    File.Delete(filePath);
                }

                throw;
            }

            await unitOfWork.SaveChangesAsync();

            return signatures;
        }

        public async Task<Signature> CreateAsync(Guid sourceId, SignatureType type, SignatureRequest signatureRequest)
        {
            var userId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.Set<User>()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);
            await signatureValidator.ValidateAsync(type, sourceId, currentUser);
            // var signatureInfo = await TryGetSignatureInfo(
            //     sourceId,
            //     type,
            //     signatureRequest.OriginalName,
            //     signatureRequest.File,
            //     currentUser
            // );
            var signature = await uploadService.UploadSignatureAsync(
                sourceId,
                type,
                signatureRequest.File,
                $"{signatureRequest.FileName}_{currentUser.Company.TIN}.sgn"
            );
            // signatureInfo.SignatureId = signature.Id;
            // signature.SignatureInfo = signatureInfo;

            try
            {
                switch (signature.Type)
                {
                    case SignatureType.Registry
                        when signature is RegistrySignature registrySignature:
                        await UpdateRegistryAsync(registrySignature.RegistryId, currentUser);
                        return await unitOfWork.AddAndSaveAsync(registrySignature);
                    case SignatureType.CompanyRegulation
                        when signature is CompanyRegulationSignature companyRegulationSignature:
                        return await unitOfWork.AddAndSaveAsync(companyRegulationSignature);
                    case SignatureType.UnformalizedDocument
                        when signature is UnformalizedDocumentSignature unformalizedDocumentSignature:
                        await UpdateUnformalizedDocumentAsync(unformalizedDocumentSignature.UnformalizedDocumentId,
                            currentUser);
                        return await unitOfWork.AddAndSaveAsync(unformalizedDocumentSignature);
                    case SignatureType.UserRegulation
                        when signature is UserRegulationSignature userProfileRegulationSignature:
                        return await unitOfWork.AddAndSaveAsync(userProfileRegulationSignature);
                    case SignatureType.Upload
                        when signature is UploadSignature uploadSignature:
                        return await unitOfWork.AddAndSaveAsync(uploadSignature);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch
            {
                var filePath = pathProviderService.GetSignaturePath(signature.Name, signature.Type);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                throw;
            }
        }

        public async Task RemoveIfAnyAsync(SignatureType type, Guid sourceId)
        {
            await ValidateSignatureAccessPermissionAsync(type, sourceId);
            Signature signature;
            switch (type)
            {
                case SignatureType.Registry:
                    signature =
                        await unitOfWork.Set<RegistrySignature>()
                            .FirstOrDefaultAsync(s => s.RegistryId == sourceId);
                    break;
                case SignatureType.CompanyRegulation:
                    signature =
                        await unitOfWork.Set<CompanyRegulationSignature>()
                            .FirstOrDefaultAsync(s => s.CompanyRegulationId == sourceId);
                    break;
                case SignatureType.UserRegulation:
                    signature =
                        await unitOfWork.Set<UserRegulationSignature>()
                            .FirstOrDefaultAsync(s => s.UserRegulationId == sourceId);
                    break;
                case SignatureType.Upload:
                    signature =
                        await unitOfWork.Set<UploadSignature>()
                            .FirstOrDefaultAsync(s => s.UploadId == sourceId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (signature != null)
            {
                await RemoveAsync(signature);
            }
        }

        private async Task RemoveAsync<T>(T signature) where T : Signature
        {
            var filePath = pathProviderService.GetSignaturePath(signature.Name, signature.Type);
            if (File.Exists(filePath))
                File.Delete(filePath);
            unitOfWork.Set<T>().Remove(signature);
            await unitOfWork.SaveChangesAsync();
        }

        private async Task UpdateRegistryAsync(Guid registryId, User currentUser)
        {
            var registry = await unitOfWork.GetOrFailAsync(registryId,
                unitOfWork.Set<Registry>()
                    .Include(r => r.Supplies)
                    .Include(r => r.Contract));
            InitRegistrySignStatus(ref registry, currentUser);
            InitRegistrySupplyStatus(ref registry);
            await InitRegistryStatusAsync(registry);
            unitOfWork.Set<Registry>().Update(registry);
        }

        private async Task UpdateUnformalizedDocumentAsync(Guid unformalizedDocumentId, User currentUser)
        {
            var unformalizedDocument = await unitOfWork.GetOrFailAsync(unformalizedDocumentId,
                unitOfWork.Set<UnformalizedDocument>()
                    .Include(r => r.Receivers));
            if (unformalizedDocument.SenderId == currentUser.CompanyId)
            {
                unformalizedDocument.IsSent = true;
                unformalizedDocument.Status = UnformalizedDocumentStatus.NeedReceiverSignature;
                unformalizedDocument.SentDate = DateTime.UtcNow;
            }
            else
            {
                if (unformalizedDocument.Receivers
                    .Where(r => r.ReceiverId != currentUser.CompanyId && r.NeedSignature)
                    .All(r => r.IsSigned))
                {
                    unformalizedDocument.Status = UnformalizedDocumentStatus.SignedByAll;
                }

                unformalizedDocument.Receivers
                    .Where(r => r.ReceiverId == currentUser.CompanyId)
                    .Select(r =>
                    {
                        r.IsSigned = true;
                        return r;
                    })
                    .ToList();
                await unitOfWork.UpdateNestedCollectionAsync(unformalizedDocument.Receivers,
                    p => p.UnformalizedDocumentId == unformalizedDocument.Id);
            }

            unitOfWork.Set<UnformalizedDocument>().Update(unformalizedDocument);
        }

        private void InitRegistrySignStatus(ref Registry registry, User currentUser)
        {
            if (registry.Contract.SellerId == currentUser.CompanyId)
            {
                registry.SignStatus = registry.SignStatus switch
                {
                    RegistrySignStatus.NotSigned => RegistrySignStatus.SignedBySeller,
                    RegistrySignStatus.SignedByBuyer => RegistrySignStatus.SignedBySellerBuyer,
                    _ => throw new ForbiddenException()
                };
            }
            else if (registry.Contract.BuyerId == currentUser.CompanyId)
            {
                registry.SignStatus = registry.SignStatus switch
                {
                    RegistrySignStatus.NotSigned => RegistrySignStatus.SignedBySeller,
                    RegistrySignStatus.SignedBySeller => RegistrySignStatus.SignedBySellerBuyer,
                    _ => throw new ForbiddenException()
                };
            }
            else if (registry.BankId.HasValue &&
                     registry.BankId.Value == currentUser.CompanyId)
            {
                registry.SignStatus = RegistrySignStatus.SignedByAll;
            }
            else
            {
                throw new ForbiddenException();
            }
        }

        private async Task InitRegistryStatusAsync(Registry registry)
        {
            switch (registry.SignStatus)
            {
                case RegistrySignStatus.NotSigned:
                case RegistrySignStatus.SignedBySeller:
                case RegistrySignStatus.SignedByBuyer:
                    registry.Status = RegistryStatus.InProcess;
                    break;
                case RegistrySignStatus.SignedBySellerBuyer:
                {
                    if (registry.FinanceType == FinanceType.DynamicDiscounting &&
                        await unitOfWork.Set<Discount>()
                            .AnyAsync(d =>
                                d.RegistryId == registry.Id &&
                                d.DiscountingSource == DiscountingSource.Personal))
                    {
                        registry.Status = RegistryStatus.Finished;
                    }
                    else
                    {
                        registry.Status = RegistryStatus.InProcess;
                    }
                }
                    break;
                case RegistrySignStatus.SignedByAll:
                    registry.IsVerified = registry.FinanceType == FinanceType.SupplyVerification;
                    registry.Status = RegistryStatus.Finished;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InitRegistrySupplyStatus(ref Registry registry)
        {
            if (registry.SignStatus != RegistrySignStatus.SignedBySellerBuyer || !registry.Supplies.Any())
                return;
            foreach (var supply in registry.Supplies)
            {
                supply.BankId = registry.BankId;
            }
        }

        private async Task ValidateSignatureAccessPermissionAsync(SignatureType type, Guid sourceId)
        {
            var userId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            await signatureValidator.ValidateAccessPermissionAsync(type, sourceId, currentUser);
        }

        private async Task<SignatureInfo> TryGetSignatureInfo(
            Guid sourceId,
            SignatureType type,
            string originalFileName,
            IFormFile file,
            User currentUser
        )
        {
            string originalFilePath = null;
            switch (type)
            {
                case SignatureType.Registry:
                    originalFilePath =
                        pathProviderService.GetRegistryTemplateDestinationPath(originalFileName);
                    break;
                case SignatureType.UnformalizedDocument:
                    var unformalizedDocumentUpload = await unitOfWork.Set<Upload>().FirstOrDefaultAsync(u =>
                        u.Provider == UploadProvider.UnformalizedDocument &&
                        u.ProviderId == sourceId &&
                        u.Name == originalFileName);
                    if (unformalizedDocumentUpload is null)
                    {
                        throw new NotFoundException(typeof(UnformalizedDocument));
                    }

                    originalFilePath = pathProviderService.GetUploadPath(
                        unformalizedDocumentUpload.Id,
                        unformalizedDocumentUpload.ContentType,
                        unformalizedDocumentUpload.Provider
                    );

                    break;
                case SignatureType.Upload:
                    var upload = await unitOfWork.GetOrFailAsync<Upload, Guid>(sourceId);
                    originalFilePath =
                        pathProviderService.GetUploadPath(upload.Id, upload.ContentType, upload.Provider);
                    break;
                case SignatureType.CompanyRegulation:
                    var companyRegulation = await unitOfWork.GetOrFailAsync<CompanyRegulation, Guid>(sourceId);
                    originalFilePath = pathProviderService.GetCompanyRegulationDestinationPath(
                        companyRegulation.Id,
                        companyRegulation.ContentType,
                        companyRegulation.Type
                    );
                    break;
                case SignatureType.UserRegulation:
                    var userRegulation = await unitOfWork.GetOrFailAsync<UserRegulation, Guid>(sourceId);
                    switch (userRegulation.Type)
                    {
                        case UserRegulationType.Profile:
                            originalFilePath =
                                pathProviderService.GetUserProfileRegulationDestinationPath(userRegulation.Id);
                            break;
                        case UserRegulationType.Other:
                            var uploadedRegulation = await unitOfWork.Set<Upload>()
                                .FirstOrDefaultAsync(u => u.Provider == UploadProvider.UserRegulation &&
                                                          u.ProviderId == sourceId &&
                                                          u.Name == originalFileName);
                            if (uploadedRegulation != null)
                            {
                                originalFilePath = pathProviderService.GetUploadPath(uploadedRegulation.Id,
                                    uploadedRegulation.ContentType, uploadedRegulation.Provider);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (string.IsNullOrEmpty(originalFilePath) || !File.Exists(originalFilePath))
            {
                throw new NotFoundException(originalFileName);
            }

            var originalFileBytes = await File.ReadAllBytesAsync(originalFilePath);
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var signatureFileBytes = ms.ToArray();
            var verificationResponse = await signatureVerifierService.GetVerificationCenterResponseAsync(
                Convert.ToBase64String(signatureFileBytes),
                Convert.ToBase64String(originalFileBytes)
            );

            if (verificationResponse == null || verificationResponse.Success == false)
            {
                throw new ValidationException("Signature",
                    verificationResponse?.Result,
                    new ForbiddenErrorDetails("wrong-signature"));
            }


            var inn = signatureVerifierService.GetInnFromVerificationResponse(verificationResponse.Data);
            var currentUserInn = currentUser.Company.TIN.Length == 10
                ? currentUser.Company.TIN.Insert(0, "00")
                : currentUser.Company.TIN;
            if (!string.IsNullOrEmpty(inn))
            {
                if (inn != currentUserInn)
                {
                    throw new ValidationException("SignatureINN",
                        $"Certificate INN({inn}) does not match with current user's company INN",
                        new GeneralErrorDetails("wrong-signer", new
                        {
                            CerificateInn = inn,
                            CurrentInn = currentUserInn
                        }));
                }
            }

            return signatureVerifierService.GetSignatureInfo(verificationResponse);
        }
    }
}