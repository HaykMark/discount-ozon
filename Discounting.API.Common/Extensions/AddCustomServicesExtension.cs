using Discounting.API.Common.Email;
using Discounting.API.Common.Services;
using Discounting.Data.Context;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics;
using Discounting.Logics.AccessControl;
using Discounting.Logics.Account;
using Discounting.Logics.CompanyServices;
using Discounting.Logics.Excel;
using Discounting.Logics.FactoringAgreements;
using Discounting.Logics.Regulations;
using Discounting.Logics.TariffDiscounting;
using Discounting.Logics.Templates;
using Discounting.Logics.Validators;
using Discounting.Logics.Validators.Account;
using Microsoft.Extensions.DependencyInjection;

namespace Discounting.API.Common.Extensions
{
    public static class AddCustomServicesExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, DiscountingDbContext>();
            //services
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IFirewall, Firewall>();
            services.AddSingleton<IZoneService, ZoneService>();
            services.AddScoped<ITokenStoreService, TokenStoreService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IUploadService, UploadService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ISupplyService, SupplyService>();
            services.AddScoped<ICompanySettingsService, CompanySettingsService>();
            services.AddScoped<IRegistryService, RegistryService>();
            services.AddScoped<ITariffService, TariffService>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<ISignatureService, SignatureService>();
            services.AddScoped<IUploadPathProviderService, UploadPathProviderService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<IBuyerTemplateConnectionService, BuyerTemplateConnectionService>();
            services.AddScoped<IRegulationService, RegulationService>();
            services.AddScoped<ICompanyRegulationService, CompanyRegulationService>();
            services.AddScoped<ICompanyAggregateService, CompanyAggregateService>();
            services.AddScoped<IUserRegulationService, UserRegulationService>();
            services.AddScoped<IUnformalizedDocumentService, UnformalizedDocumentService>();
            services.AddScoped<ICalendarService, CalendarService>();
            services.AddScoped<IDiscountSettingsService, DiscountSettingsService>();
            services.AddScoped<IFactoringAgreementService, FactoringAgreementService>();
            services.AddScoped<ISupplyFactoringAgreementService, SupplyFactoringAgreementService>();
            services.AddScoped<ICompanyBankService, CompanyBankService>();

            //Excel services
            services.AddScoped<IExcelDocumentReaderService, ExcelDocumentReaderService>();
            services.AddScoped<IExcelDocumentGeneratorService, ExcelDocumentGeneratorService>();
            
            //Validators
            services.AddScoped<ICommonValidations, CommonValidations>();
            services.AddScoped<IUserValidator, UserValidator>();
            services.AddScoped<ICompanyValidator, CompanyValidator>();
            services.AddScoped<IContractValidator, ContractValidator>();
            services.AddScoped<ISupplyValidator, SupplyValidator>();
            services.AddScoped<IRegistryValidator, RegistryValidator>();
            services.AddScoped<ITariffValidator, TariffValidator>();
            services.AddScoped<IDiscountValidator, DiscountValidator>();
            services.AddScoped<ISignatureValidator, SignatureValidator>();
            services.AddScoped<IUserRoleValidator, UserRoleValidator>();
            services.AddScoped<ITemplateValidator, TemplateValidator>();
            services.AddScoped<IBuyerTemplateConnectionValidator, BuyerTemplateConnectionValidator>();
            services.AddScoped<ICompanyRegulationValidator, CompanyRegulationValidator>();
            services.AddScoped<IReferenceValidator, ReferenceValidator>();
            services.AddScoped<IUnformalizedDocumentValidator, UnformalizedDocumentValidator>();
            services.AddScoped<IUserProfileRegulationInfoValidator, UserProfileRegulationInfoValidator>();
            services.AddScoped<IUserRegulationValidator, UserRegulationValidator>();
            services.AddScoped<IDiscountSettingsValidator, DiscountSettingsValidator>();
            services.AddScoped<IFactoringAgreementValidator, FactoringAgreementValidator>();
            
            //Web Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ISupplyExcelParsingService, SupplyExcelParsingService>();
            services.AddScoped<ITokenValidatorService, TokenValidatorService>();
            services.AddSingleton<IMailer, Mailer>();

            return services;
        }
    }
}
