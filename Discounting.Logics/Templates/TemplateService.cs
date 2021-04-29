using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities.Templates;
using Discounting.Logics.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Templates
{
    public interface ITemplateService
    {
        Task<Template> Get(Guid id);
        Task<Template[]> GetAllAsync(Guid? companyId, TemplateType? type);
        Task<Template> CreateAsync(Guid companyId, TemplateType type, IFormFile file);
        Task<Template> UpdateAsync(Guid id, Guid companyId, TemplateType type, IFormFile file);
        Task RemoveAsync(Guid id);
    }

    public class TemplateService : ITemplateService
    {
        private readonly IUploadService uploadService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUploadPathProviderService pathProviderService;
        private readonly ITemplateValidator templateValidator;

        public TemplateService(
            IUnitOfWork unitOfWork,
            IUploadService uploadService,
            IUploadPathProviderService pathProviderService,
            ITemplateValidator templateValidator
        )
        {
            this.unitOfWork = unitOfWork;
            this.uploadService = uploadService;
            this.pathProviderService = pathProviderService;
            this.templateValidator = templateValidator;
        }

        public Task<Template[]> GetAllAsync(Guid? companyId, TemplateType? type)
        {
            return unitOfWork.Set<Template>().Where(t =>
                    (!type.HasValue || t.Type == type.Value) &&
                    (!companyId.HasValue || t.CompanyId == companyId))
                .ToArrayAsync();
        }

        public Task<Template> Get(Guid id) =>
            unitOfWork.GetOrFailAsync<Template, Guid>(id);


        public async Task<Template> CreateAsync(Guid companyId, TemplateType type, IFormFile file)
        {
            await templateValidator.ValidateAsync(Guid.Empty, companyId, type);
            var template = await uploadService.UploadTemplateAsync(companyId, type, file);
            try
            {
                await unitOfWork.AddAndSaveAsync(template);
                return template;
            }
            catch
            {
                var filePath = pathProviderService.GetTemplatePath(template.Id, template.ContentType, template.Type);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                throw;
            }
        }

        public async Task<Template> UpdateAsync(Guid id, Guid companyId, TemplateType type, IFormFile file)
        {
            var template = await unitOfWork.GetOrFailAsync<Template, Guid>(id);
            await templateValidator.ValidateAsync(id, companyId, type);
            template.Name = file.FileName;
            template.Size = file.Length;
            template.CompanyId = companyId;
            template.ContentType = file.ContentType;
            template.Type = type;

            var filePath = pathProviderService.GetTemplatePath(template.Id, template.ContentType, template.Type);
            await uploadService.SaveFileAsync(filePath, file);

            unitOfWork.Set<Template>().Update(template);
            await unitOfWork.SaveChangesAsync();
            return template;
        }

        public async Task RemoveAsync(Guid id)
        {
            var template = await unitOfWork.GetOrFailAsync<Template, Guid>(id);
            await templateValidator.ValidateRemoval(template);
            var filePath = pathProviderService.GetTemplatePath(template.Id, template.ContentType, template.Type);
            unitOfWork.Set<Template>().Remove(template);
            await unitOfWork.SaveChangesAsync();
            if (File.Exists(Path.Combine(filePath)))
                File.Delete(Path.Combine(filePath));
        }
    }
}