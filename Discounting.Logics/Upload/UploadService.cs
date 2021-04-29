using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Response;
using Discounting.Common.Validation;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.Auditing;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Templates;
using Discounting.Helpers;
using Discounting.Logics.Account;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics
{
    public interface IUploadService
    {
        Task SaveFileAsync(string filePath, IFormFile file);
        Task<IReadOnlyCollection<Upload>> GetAllAsync(Expression<Func<Upload, bool>> filter);
        Task<ZipItem[]> GetZipItemsAsync(UploadProvider provider, Guid providerId);
        ZipItem[] GetZipItems(Upload[] uploads);
        Task<Upload> GetAsync(Guid uploadId);
        Task<Upload> UploadFileAsync(Guid providerId, UploadProvider provider, IFormFile file);
        Task<Upload> UpdateFileAsync(Guid id, Guid providerId, UploadProvider provider, IFormFile file);

        Task<List<Upload>> UploadFilesAsync(
            Guid providerId,
            UploadProvider provider,
            IFormFile[] files
        );

        Task<Template> UploadTemplateAsync(
            Guid companyId,
            TemplateType type,
            IFormFile file
        );

        Task<CompanyRegulation> UploadCompanyRegulationAsync(
            Guid userId,
            Guid companyId,
            CompanyRegulationType type,
            IFormFile file
        );

        Task<Signature> UploadSignatureAsync(Guid sourceId, SignatureType type, IFormFile file, string fileName);
        Task<Upload> RemoveAsync(Guid id);
        Task RemoveAsync(List<Upload> uploads);
    }

    public class UploadService : IUploadService
    {
        private readonly ISessionService sessionService;
        private readonly IUnitOfWork unitOfWork;
        private readonly DbSet<Upload> baseDbSet;
        private readonly IUploadPathProviderService uploadPathProviderService;

        public UploadService(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            IUploadPathProviderService uploadPathProviderService
        )
        {
            this.unitOfWork = unitOfWork;
            this.sessionService = sessionService;
            this.uploadPathProviderService = uploadPathProviderService;
            baseDbSet = unitOfWork.Set<Upload>();
        }

        public async Task<IReadOnlyCollection<Upload>> GetAllAsync(Expression<Func<Upload, bool>> filter)
        {
            // TODO: filter by userId, if there may be a requirement for.
            // var userId = this.userService.GetCurrentUserId();
            var uploads = await baseDbSet
                .Where(filter)
                .ToArrayAsync();
            return new ReadOnlyCollection<Upload>(uploads);
        }

        public async Task<ZipItem[]> GetZipItemsAsync(UploadProvider provider, Guid providerId)
        {
            var uploads = await baseDbSet
                .Where(u => u.Provider == provider &&
                            u.ProviderId == providerId)
                .ToArrayAsync();

            return uploads.Select(u => new ZipItem
                {
                    Path = uploadPathProviderService.GetUploadPath(u.Id, u.ContentType, u.Provider),
                    Name = u.Name.Replace(' ', '_')
                })
                .ToArray();
        }

        public ZipItem[] GetZipItems(Upload[] uploads)
        {
            return uploads.Select(u => new ZipItem
                {
                    Path = uploadPathProviderService.GetUploadPath(u.Id, u.ContentType, u.Provider),
                    Name = u.Name.Replace(' ', '_')
                })
                .ToArray();
        }

        public Task<Upload> GetAsync(Guid uploadId) =>
            unitOfWork.GetOrFailAsync<Upload, Guid>(uploadId);

        public async Task<Upload> UploadFileAsync(Guid providerId, UploadProvider provider, IFormFile file)
        {
            var userId = sessionService.GetCurrentUserId();
            var upload = await UploadAsync(userId, providerId, provider, file);
            return await unitOfWork.AddAndSaveAsync(upload);
        }

        public async Task<Upload> UpdateFileAsync(Guid id, Guid providerId, UploadProvider provider, IFormFile file)
        {
            var upload = await GetAsync(id);
            await RewriteAsync(upload, providerId, provider, file);
            return await unitOfWork.UpdateAndSaveAsync<Upload, Guid>(upload);
        }

        public async Task<List<Upload>> UploadFilesAsync(
            Guid providerId,
            UploadProvider provider,
            IFormFile[] files
        )
        {
            //if total size is more than 150MB throw and exception 
            if (files.Sum(f => f.Length) > 157286400)
            {
                throw new ValidationException("file",
                    "Total size of all files cannot exceed 150MB",
                    new ForbiddenErrorDetails("files-size-is-more-than-150-mb"));
            }

            var userId = sessionService.GetCurrentUserId();
            var uploads = new List<Upload>();
            foreach (var file in files)
            {
                uploads.Add(await UploadAsync(userId, providerId, provider, file));
            }

            if (!uploads.Any())
            {
                throw new NotFoundException();
            }

            var createdUploads = await unitOfWork.AddRangeAndSaveAsync(uploads);
            return createdUploads.ToList();
        }

        private async Task<Upload> UploadAsync(Guid userId, Guid providerId, UploadProvider provider, IFormFile file)
        {
            if (file.Length == 0)
            {
                throw new ValidationException(new RequiredFieldValidationError("file"));
            }

            var upload = new Upload
            {
                Id = Guid.NewGuid(),
                Name = file.FileName,
                CreatedDate = DateTime.UtcNow,
                Size = file.Length,
                UserId = userId,
                ContentType = file.ContentType,
                Provider = provider,
                ProviderId = providerId
            };
            var filePath = uploadPathProviderService.GetUploadPath(upload.Id, upload.ContentType, upload.Provider);
            await SaveFileAsync(filePath, file);
            return upload;
        }

        private async Task RewriteAsync(Upload upload, Guid providerId, UploadProvider provider, IFormFile file)
        {
            if (file.Length == 0)
            {
                throw new ValidationException(new RequiredFieldValidationError("file"));
            }

            RemoveUploadedFileIfExists(upload.Id, upload.Provider, upload.ContentType);

            upload.Name = file.FileName;
            upload.Size = file.Length;
            upload.ContentType = file.ContentType;
            upload.Provider = provider;
            upload.ProviderId = providerId;

            var newFilePath = uploadPathProviderService.GetUploadPath(upload.Id, upload.ContentType, provider);
            await SaveFileAsync(newFilePath, file);
        }

        public async Task<Template> UploadTemplateAsync(
            Guid companyId,
            TemplateType type,
            IFormFile file)
        {
            if (file.Length == 0)
            {
                // skip empty files
                throw new ValidationException(new RequiredFieldValidationError("file"));
            }

            var template = new Template
            {
                Id = Guid.NewGuid(),
                Name = file.FileName,
                CreatedDate = DateTime.UtcNow,
                Size = file.Length,
                CompanyId = companyId,
                ContentType = file.ContentType,
                Type = type
            };
            var filePath = uploadPathProviderService.GetTemplatePath(template.Id, template.ContentType, template.Type);

            await SaveFileAsync(filePath, file);
            return template;
        }

        public async Task<CompanyRegulation> UploadCompanyRegulationAsync(
            Guid userId,
            Guid companyId,
            CompanyRegulationType type,
            IFormFile file)
        {
            if (file.Length == 0)
            {
                // skip empty files
                throw new ValidationException(new RequiredFieldValidationError("file"));
            }

            var companyRegulation = new CompanyRegulation
            {
                Id = Guid.NewGuid(),
                Name = file.FileName,
                CreatedDate = DateTime.UtcNow,
                Size = file.Length,
                UserId = userId,
                CompanyId = companyId,
                ContentType = file.ContentType,
                Type = type
            };
            var filePath = uploadPathProviderService.GetCompanyRegulationDestinationPath(companyRegulation.Id,
                companyRegulation.ContentType, companyRegulation.Type);

            await SaveFileAsync(filePath, file);
            return companyRegulation;
        }

        public async Task<Signature> UploadSignatureAsync(Guid sourceId, SignatureType type, IFormFile file,
            string fileName)
        {
            if (file.Length == 0)
            {
                // skip empty files
                throw new ValidationException(new RequiredFieldValidationError("file"));
            }

            var userId = sessionService.GetCurrentUserId();
            Signature signature = type switch
            {
                SignatureType.Registry => new RegistrySignature
                {
                    RegistryId = sourceId
                },
                SignatureType.UnformalizedDocument => new UnformalizedDocumentSignature
                {
                    UnformalizedDocumentId = sourceId
                },
                SignatureType.Upload => new UploadSignature
                {
                    UploadId = sourceId
                },
                SignatureType.CompanyRegulation => new CompanyRegulationSignature
                {
                    CompanyRegulationId = sourceId
                },
                SignatureType.UserRegulation => new UserRegulationSignature
                {
                    UserRegulationId = sourceId
                },
                _ => throw new ForbiddenException()
            };

            signature.Id = Guid.NewGuid();
            signature.Name = fileName;
            signature.Size = file.Length;
            signature.SignerId = userId;
            signature.CreationDate = DateTime.UtcNow;
            signature.Type = type;

            var filePath = uploadPathProviderService.GetSignaturePath(fileName, signature.Type);
            await SaveFileAsync(filePath, file);
            return signature;
        }

        public async Task SaveFileAsync(string filePath, IFormFile file)
        {
            if (File.Exists(Path.Combine(filePath)))
                File.Delete(Path.Combine(filePath));

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        public async Task<Upload> RemoveAsync(Guid id)
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            var upload = await unitOfWork.GetOrFailAsync<Upload, Guid>(id);
            switch (upload.Provider)
            {
                case UploadProvider.Regulation when !user.IsSuperAdmin:
                case UploadProvider.UserRegulation when !user.IsAdmin:
                    throw new ForbiddenException();
                case UploadProvider.UnformalizedDocument:
                    if (await unitOfWork.Set<UnformalizedDocumentSignature>()
                        .AnyAsync(d => d.UnformalizedDocumentId == upload.ProviderId))
                    {
                        throw new ForbiddenException();
                    }

                    break;
                case UploadProvider.Registry:
                {
                    if (await unitOfWork.Set<RegistrySignature>()
                        .AnyAsync(d => d.RegistryId == upload.ProviderId))
                    {
                        throw new ForbiddenException();
                    }

                    break;
                }
            }

            unitOfWork.Set<Upload>().Remove(upload);
            await unitOfWork.SaveChangesAsync();

            RemoveUploadedFileIfExists(upload.Id, upload.Provider, upload.ContentType);
            return upload;
        }

        // Use this method only for those type of providers who have multiple files/uploads for the same providerId
        public async Task RemoveAsync(List<Upload> uploads)
        {
            if (!uploads.Any())
                return;
            var provider = uploads.First().Provider;
            var providerId = uploads.First().ProviderId;
            switch (provider)
            {
                case UploadProvider.Registry:
                {
                    if (await unitOfWork.Set<RegistrySignature>()
                        .AnyAsync(d => d.RegistryId == providerId))
                    {
                        throw new ForbiddenException();
                    }

                    break;
                }
                case UploadProvider.UnformalizedDocument:
                    if (await unitOfWork.Set<UnformalizedDocumentSignature>()
                        .AnyAsync(d => d.UnformalizedDocumentId == providerId))
                    {
                        throw new ForbiddenException();
                    }

                    break;
                default:
                    throw new ArgumentException();
            }

            unitOfWork.Set<Upload>().RemoveRange(uploads);
            await unitOfWork.SaveChangesAsync();
            uploads.ForEach(u => RemoveUploadedFileIfExists(u.Id, u.Provider, u.ContentType));
        }

        private void RemoveUploadedFileIfExists(Guid id, UploadProvider provider, string contentType)
        {
            var filePath = uploadPathProviderService.GetUploadPath(id, contentType, provider);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}