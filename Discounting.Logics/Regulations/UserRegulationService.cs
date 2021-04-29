using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.Regulations;
using Discounting.Entities.Templates;
using Discounting.Helpers;
using Discounting.Logics.Account;
using Discounting.Logics.Excel;
using Discounting.Logics.Validators;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Regulations
{
    public interface IUserRegulationService
    {
        Task<UserRegulation[]> GetAllAsync(Guid? userId = null);
        Task<UserRegulation> GetAsync(Guid id);
        Task<string> GetProfileFileAsync(Guid id);
        Task<UserRegulation> CreateAsync(UserRegulation entity);
        Task<UserProfileRegulationInfo> UpdateProfileAsync(UserProfileRegulationInfo entity);
        Task RemoveAsync(Guid id);
        Task<IEnumerable<ZipItem>> GetZipItemsAsync(Guid userId);
    }

    public class UserRegulationService : IUserRegulationService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserRegulationValidator regulationValidator;
        private readonly ISessionService sessionService;
        private readonly IUploadPathProviderService pathProviderService;
        private readonly ISignatureService signatureService;
        private readonly IExcelDocumentGeneratorService excelDocumentGeneratorService;
        private readonly IUploadService uploadService;

        public UserRegulationService(
            IUnitOfWork unitOfWork,
            IUserRegulationValidator regulationValidator,
            ISessionService sessionService,
            IUploadPathProviderService pathProviderService,
            ISignatureService signatureService,
            IExcelDocumentGeneratorService excelDocumentGeneratorService,
            IUploadService uploadService
        )
        {
            this.unitOfWork = unitOfWork;
            this.regulationValidator = regulationValidator;
            this.sessionService = sessionService;
            this.pathProviderService = pathProviderService;
            this.signatureService = signatureService;
            this.excelDocumentGeneratorService = excelDocumentGeneratorService;
            this.uploadService = uploadService;
        }

        private IQueryable<UserRegulation> GetBaseQuery() =>
            unitOfWork.Set<UserRegulation>().Include(u => u.UserProfileRegulationInfo);


        public async Task<UserRegulation[]> GetAllAsync(Guid? userId = null)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            if (userId.HasValue)
            {
                var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId.Value);
                if (!currentUser.IsSuperAdmin && user.CompanyId != currentUser.CompanyId)
                {
                    throw new NotFoundException();
                }

                return await GetBaseQuery()
                    .Where(u => u.UserId == userId.Value)
                    .ToArrayAsync();
            }

            return await GetBaseQuery()
                .Where(u => u.User.CompanyId == currentUser.CompanyId)
                .ToArrayAsync();
        }

        public async Task<UserRegulation> GetAsync(Guid id)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            if (!currentUser.IsSuperAdmin)
            {
                await regulationValidator.ValidateRequestedRegulationPermissionAsync(id, currentUser.CompanyId);
            }

            return await unitOfWork.GetOrFailAsync(id, GetBaseQuery());
        }

        public async Task<string> GetProfileFileAsync(Guid id)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            await regulationValidator.ValidateForProfileTemplateAsync(
                id,
                currentUser
            );
            var userRegulation = await unitOfWork.GetOrFailAsync(id, GetBaseQuery()
                .Include(u => u.User)
                .ThenInclude(u => u.Company)
                .ThenInclude(c => c.CompanyContactInfo));

            if (!await unitOfWork.Set<Template>().AnyAsync(t => t.Type == TemplateType.ProfileRegulationUser))
            {
                throw new NotFoundException(typeof(Template));
            }

            return await excelDocumentGeneratorService.GetOrGenerateUserProfileRegulationExcelAsync(userRegulation);
        }

        public async Task<UserRegulation> CreateAsync(UserRegulation entity)
        {
            await regulationValidator.ValidateAsync(entity);
            return await unitOfWork.AddAndSaveAsync(entity);
        }

        public async Task<UserProfileRegulationInfo> UpdateProfileAsync(UserProfileRegulationInfo entity)
        {
            regulationValidator.ValidateProfile(entity);
            var filePath = pathProviderService.GetUserProfileRegulationDestinationPath(entity.UserRegulationId);
            if (File.Exists(Path.Combine(filePath)))
            {
                File.Delete(Path.Combine(filePath));
            }

            await signatureService.RemoveIfAnyAsync(SignatureType.UserRegulation, entity.UserRegulationId);

            return await unitOfWork.UpdateAndSaveAsync<UserProfileRegulationInfo, Guid>(entity);
        }

        public async Task RemoveAsync(Guid id)
        {
            var userRegulation = await unitOfWork.GetOrFailAsync<UserRegulation, Guid>(id);
            await RemoveUserRegulationFileIfAnyAsync(userRegulation);
            await signatureService.RemoveIfAnyAsync(SignatureType.UserRegulation, id);
            unitOfWork.Set<UserRegulation>().Remove(userRegulation);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ZipItem>> GetZipItemsAsync(Guid userId)
        {
            var allZipItems = new List<ZipItem>();
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            if (!currentUser.IsSuperAdmin && user.CompanyId != currentUser.CompanyId)
            {
                throw new NotFoundException();
            }

            var userRegulations = await unitOfWork.Set<UserRegulation>()
                .Where(u => u.UserId == userId)
                .ToArrayAsync();
            var regulationIds = userRegulations
                .Select(u => u.Id)
                .ToArray();
            var profile = userRegulations.FirstOrDefault(p => p.Type == UserRegulationType.Profile);
            var others = userRegulations
                .Where(r => r.Type == UserRegulationType.Other)
                .Select(r => r.Id)
                .ToList();
            var uploads = await unitOfWork.Set<Upload>()
                .Where(u => u.Provider == UploadProvider.UserRegulation && others.Contains(u.ProviderId))
                .ToArrayAsync();

            Signature[] signatures = await unitOfWork.Set<UserRegulationSignature>()
                .Include(s => s.Signer)
                .ThenInclude(s => s.Company)
                .Where(s => s.Type == SignatureType.UserRegulation &&
                            regulationIds.Contains(s.UserRegulationId))
                .ToArrayAsync();

            var uploadZipItems = uploadService.GetZipItems(uploads);
            if (uploadZipItems.Any())
            {
                allZipItems.AddRange(uploadZipItems);
            }

            ZipItem profileZipItem = null;
            if (profile != null)
            {
                var profileFilePath = await GetProfileFileAsync(profile.Id);
                profileZipItem = new ZipItem
                {
                    Path = profileFilePath,
                    Name = "Анкета.xlsx"
                };
            }

            if (profileZipItem != null)
            {
                allZipItems.Add(profileZipItem);
            }

            if (signatures.Any())
            {
                allZipItems.AddRange(
                    signatureService.TryGetSignatureZipItems(signatures, SignatureType.UserRegulation));
            }

            return allZipItems;
        }

        private async Task RemoveUserRegulationFileIfAnyAsync(UserRegulation userRegulation)
        {
            var filePath = string.Empty;

            switch (userRegulation.Type)
            {
                case UserRegulationType.Profile:
                    filePath = pathProviderService.GetUserProfileRegulationDestinationPath(userRegulation.Id);
                    break;
                case UserRegulationType.Other:
                    var upload = await unitOfWork.Set<Upload>().FirstOrDefaultAsync(u =>
                        u.Provider == UploadProvider.UserRegulation &&
                        u.ProviderId == userRegulation.Id);
                    if (upload != null)
                    {
                        filePath = pathProviderService.GetUploadPath(upload.Id, upload.ContentType,
                            UploadProvider.UserRegulation);
                        unitOfWork.Set<Upload>().Remove(upload);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!string.IsNullOrEmpty(filePath) && File.Exists(Path.Combine(filePath)))
            {
                File.Delete(Path.Combine(filePath));
            }
        }
    }
}