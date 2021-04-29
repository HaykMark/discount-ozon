using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Report;
using Discounting.Entities;
using Discounting.Data.Context;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;
using Discounting.Entities.Templates;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Excel
{
    public interface IExcelDocumentGeneratorService
    {
        Task<string> GetOrGenerateRegistryTemplateAsync(Registry registry, TemplateType type);

        Task<string> GetOrGenerateCompanyProfileRegulationExcelAsync(Guid userId, Company company);
        Task<string> GetOrGenerateUserProfileRegulationExcelAsync(UserRegulation userRegulation);
    }

    public class ExcelDocumentGeneratorService : IExcelDocumentGeneratorService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUploadPathProviderService pathProviderService;

        public ExcelDocumentGeneratorService(
            IUnitOfWork unitOfWork,
            IUploadPathProviderService pathProviderService
        )
        {
            this.unitOfWork = unitOfWork;
            this.pathProviderService = pathProviderService;
        }

        public async Task<string> GetOrGenerateRegistryTemplateAsync(Registry registry, TemplateType type)
        {
            Template template;
            if (registry.FinanceType == FinanceType.DynamicDiscounting)
            {
                template = await unitOfWork.Set<Template>()
                    .FirstOrDefaultAsync(t => t.Type == type &&
                                              t.CompanyId == registry.Contract.BuyerId);
            }
            else
            {
                template = await unitOfWork.Set<BuyerTemplateConnection>()
                    .Include(t => t.Template)
                    .Where(t => t.Template.Type == type &&
                                t.BuyerId == registry.Contract.BuyerId)
                    .Select(t => t.Template)
                    .FirstOrDefaultAsync();
            }

            var templatePath = pathProviderService.GetTemplatePath(template.Id, template.ContentType, template.Type);

            var destinationPath = pathProviderService.GetRegistryTemplateDestinationPath(registry.GetFileName(type));
            if (registry.SignStatus == RegistrySignStatus.NotSigned)
            {
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                var registryReport = ExcelFactory.CreateExcelReport(registry, ExcelReportType.Registry);
                CreateExcelViaTemplate(registryReport, templatePath, destinationPath);
            }

            return destinationPath;
        }

        public async Task<string> GetOrGenerateCompanyProfileRegulationExcelAsync(Guid userId, Company company)
        {
            var profileRegulationTemplate = await unitOfWork.Set<CompanyRegulation>()
                .FirstOrDefaultAsync(c => c.Type == CompanyRegulationType.Profile &&
                                          c.CompanyId == company.Id);
            if (profileRegulationTemplate != null)
            {
                var prevRegulationPath = pathProviderService.GetCompanyRegulationDestinationPath(
                    profileRegulationTemplate.Id,
                    profileRegulationTemplate.ContentType, profileRegulationTemplate.Type);
                if (File.Exists(prevRegulationPath))
                {
                    File.Delete(prevRegulationPath);
                }

                unitOfWork.Set<CompanyRegulation>().Remove(profileRegulationTemplate);
            }

            var templateType = company.CompanyType switch
            {
                CompanyType.SellerBuyer => company.TIN.Length == 10
                    ? TemplateType.ProfileRegulationSellerBuyer
                    : TemplateType.ProfileRegulationPrivateCompany,
                CompanyType.Bank => TemplateType.ProfileRegulationBank,
                _ => throw new ArgumentOutOfRangeException()
            };
            var profileTemplate =
                await unitOfWork.Set<Template>().FirstAsync(t => t.Type == templateType);
            var templatePath = pathProviderService.GetTemplatePath(profileTemplate.Id,
                profileTemplate.ContentType, profileTemplate.Type);
            var fileInfo = new FileInfo(templatePath);
            var companyRegulation = new CompanyRegulation
            {
                Id = Guid.NewGuid(),
                Name = "ReportRegulation",
                CreatedDate = DateTime.UtcNow,
                Size = fileInfo.Length,
                UserId = userId,
                CompanyId = company.Id,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Type = CompanyRegulationType.Profile
            };
            var destinationPath = pathProviderService.GetCompanyRegulationDestinationPath(companyRegulation.Id,
                companyRegulation.ContentType, companyRegulation.Type);

            var profileRegulationReport = ExcelFactory.CreateExcelReport(company, ExcelReportType.ProfileRegulation);
            CreateExcelViaTemplate(profileRegulationReport, templatePath, destinationPath);
            await unitOfWork.AddAndSaveAsync(companyRegulation);
            return destinationPath;
        }

        public async Task<string> GetOrGenerateUserProfileRegulationExcelAsync(UserRegulation userRegulation)
        {
            var prevRegulationPath = pathProviderService.GetUserProfileRegulationDestinationPath(userRegulation.Id);
            if (await unitOfWork.Set<UserRegulationSignature>()
                    .AnyAsync(u => u.UserRegulationId == userRegulation.Id) &&
                File.Exists(prevRegulationPath))
            {
                return prevRegulationPath;
            }

            if (File.Exists(prevRegulationPath))
            {
                File.Delete(prevRegulationPath);
            }

            var profileTemplate =
                await unitOfWork.Set<Template>().FirstAsync(t => t.Type == TemplateType.ProfileRegulationUser);
            var templatePath = pathProviderService.GetTemplatePath(profileTemplate.Id,
                profileTemplate.ContentType, profileTemplate.Type);

            var destinationPath = pathProviderService.GetUserProfileRegulationDestinationPath(userRegulation.Id);

            var profileRegulationReport =
                ExcelFactory.CreateExcelReport(userRegulation, ExcelReportType.UserProfileRegulation);
            CreateExcelViaTemplate(profileRegulationReport, templatePath, destinationPath);
            return destinationPath;
        }

        public void CreateExcelViaTemplate(IExcelReport report, string templatePath, string destinationPath)
        {
            using var template = new XLTemplate(templatePath);
            template.AddVariable(report);
            template.Generate();
            template.Workbook.Worksheet(1).Rows();
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            template.SaveAs(destinationPath);
        }
    }
}