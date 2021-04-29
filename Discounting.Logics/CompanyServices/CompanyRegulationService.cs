using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Templates;
using Discounting.Logics.Account;
using Discounting.Logics.Excel;
using Discounting.Logics.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.CompanyServices
{
    public interface ICompanyRegulationService
    {
        Task<CompanyRegulation[]> GetAllAsync(CompanyRegulationType? type, Guid? companyId);
        Task<CompanyRegulation> Get(Guid uploadId);
        Task<string> GetProfileFileAsync();
        Task<CompanyRegulation> CreateAsync(CompanyRegulationType type, IFormFile file);
        Task<CompanyRegulation> UpdateAsync(Guid id, CompanyRegulationType type, IFormFile file);
        Task RemoveAsync(Guid id);
    }

    public class CompanyRegulationService : ICompanyRegulationService
    {
        private readonly IUploadService uploadService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;
        private readonly IUploadPathProviderService pathProviderService;
        private readonly ISignatureService signatureService;
        private readonly IExcelDocumentGeneratorService excelDocumentGeneratorService;
        private readonly ICompanyRegulationValidator regulationValidator;

        public CompanyRegulationService(
            IUnitOfWork unitOfWork,
            IUploadService uploadService,
            IUploadPathProviderService pathProviderService,
            ISessionService sessionService,
            ISignatureService signatureService,
            IExcelDocumentGeneratorService excelDocumentGeneratorService,
            ICompanyRegulationValidator regulationValidator
        )
        {
            this.unitOfWork = unitOfWork;
            this.uploadService = uploadService;
            this.pathProviderService = pathProviderService;
            this.sessionService = sessionService;
            this.signatureService = signatureService;
            this.excelDocumentGeneratorService = excelDocumentGeneratorService;
            this.regulationValidator = regulationValidator;
        }

        public Task<CompanyRegulation[]> GetAllAsync(CompanyRegulationType? type, Guid? companyId)
        {
            return unitOfWork.Set<CompanyRegulation>().Where(t =>
                    (!type.HasValue || t.Type == type.Value) &&
                    (!companyId.HasValue || t.CompanyId == companyId.Value))
                .ToArrayAsync();
        }

        public Task<CompanyRegulation> Get(Guid uploadId) =>
            unitOfWork.GetOrFailAsync<CompanyRegulation, Guid>(uploadId);

        public async Task<string> GetProfileFileAsync()
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            var company = await unitOfWork.Set<Company>()
                .Include(c => c.CompanyAuthorizedUserInfo)
                .Include(c => c.CompanyContactInfo)
                .Include(c => c.CompanyOwnerPositionInfo)
                .Include(c => c.MigrationCardInfos)
                .Include(c => c.ResidentPassportInfos)
                .FirstOrDefaultAsync(c => c.Id == user.CompanyId);

            var templateType = TemplateType.ProfileRegulationBank;
            if (company.CompanyType == CompanyType.SellerBuyer)
            {
                templateType = company.TIN.Length == 10
                    ? TemplateType.ProfileRegulationSellerBuyer
                    : TemplateType.ProfileRegulationPrivateCompany;
            }

            if (!await unitOfWork.Set<Template>().AnyAsync(t => t.Type == templateType))
            {
                throw new NotFoundException(typeof(Template));
            }

            await regulationValidator.ValidateForProfileTemplate(company);

            return await excelDocumentGeneratorService.GetOrGenerateCompanyProfileRegulationExcelAsync(userId, company);
        }

        public async Task<CompanyRegulation> CreateAsync(CompanyRegulationType type, IFormFile file)
        {
            var user = await TryGetCurrentAdminUserAsync();
            var companyRegulation =
                await uploadService.UploadCompanyRegulationAsync(user.Id, user.CompanyId, type, file);
            try
            {
                await unitOfWork.AddAndSaveAsync(companyRegulation);
                return companyRegulation;
            }
            catch
            {
                var filePath = pathProviderService.GetCompanyRegulationDestinationPath(companyRegulation.Id,
                    companyRegulation.ContentType, companyRegulation.Type);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                throw;
            }
        }

        public async Task<CompanyRegulation> UpdateAsync(Guid id, CompanyRegulationType type, IFormFile file)
        {
            var companyRegulation = await unitOfWork.GetOrFailAsync<CompanyRegulation, Guid>(id);
            var user = await TryGetCurrentAdminUserAsync();
            companyRegulation.Name = file.FileName;
            companyRegulation.Size = file.Length;
            companyRegulation.CompanyId = user.CompanyId;
            companyRegulation.UserId = user.Id;
            companyRegulation.ContentType = file.ContentType;
            companyRegulation.Type = type;

            var filePath = pathProviderService.GetCompanyRegulationDestinationPath(companyRegulation.Id,
                companyRegulation.ContentType, companyRegulation.Type);

            await uploadService.SaveFileAsync(filePath, file);
            await signatureService.RemoveIfAnyAsync(SignatureType.CompanyRegulation, id);
            unitOfWork.Set<CompanyRegulation>().Update(companyRegulation);

            await unitOfWork.SaveChangesAsync();
            return companyRegulation;
        }

        public async Task RemoveAsync(Guid id)
        {
            var companyRegulation = await unitOfWork.GetOrFailAsync<CompanyRegulation, Guid>(id);
            var filePath = pathProviderService.GetCompanyRegulationDestinationPath(companyRegulation.Id,
                companyRegulation.ContentType, companyRegulation.Type);
            if (File.Exists(Path.Combine(filePath)))
                File.Delete(Path.Combine(filePath));
            await signatureService.RemoveIfAnyAsync(SignatureType.CompanyRegulation, id);
            unitOfWork.Set<CompanyRegulation>().Remove(companyRegulation);
            await unitOfWork.SaveChangesAsync();
        }

        private async Task<User> TryGetCurrentAdminUserAsync()
        {
            var userId = sessionService.GetCurrentUserId();
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            if (!user.IsAdmin)
            {
                throw new ForbiddenException();
            }

            return user;
        }
    }
}