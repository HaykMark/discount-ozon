using Discounting.Tests.ClientApi;
using Discounting.Tests.ClientApi.AccessControl;
using Discounting.Tests.ClientApi.Account;
using Discounting.Tests.ClientAPI.CompanyAggregates;
using Discounting.Tests.ClientAPI.TariffDiscount;

namespace Discounting.Tests.Fixtures.Common
{
    public partial class BaseFixture
    {
        public IUserApi UserApi => GenerateClient<IUserApi>();

        public IRoleApi RoleApi => GenerateClient<IRoleApi>();

        public IUploadApi UploadApi => GenerateClient<IUploadApi>();

        public IAccountApi AccountApi => GenerateClient<IAccountApi>();

        public IContractApi ContractApi => GenerateClient<IContractApi>();

        public ISupplyApi SupplyApi => GenerateClient<ISupplyApi>();

        public IFactoringAgreementApi FactoringAgreementApi => GenerateClient<IFactoringAgreementApi>();

        public IRegistryApi RegistryApi => GenerateClient<IRegistryApi>();
        public ISignatureApi SignatureApi => GenerateClient<ISignatureApi>();

        public ITariffApi TariffApi => GenerateClient<ITariffApi>();

        public IDiscountApi DiscountApi => GenerateClient<IDiscountApi>();

        public IRegulationApi RegulationApi => GenerateClient<IRegulationApi>();

        //Company aggregates
        public ICompanyApi CompanyApi => GenerateClient<ICompanyApi>();
        public ICompanyAuthorizedUserApi CompanyAuthorizedUserApi => GenerateClient<ICompanyAuthorizedUserApi>();
        public ICompanyContactApi CompanyContactApi => GenerateClient<ICompanyContactApi>();
        public ICompanyOwnerPositionApi CompanyOwnerPositionApi => GenerateClient<ICompanyOwnerPositionApi>();
        public IMigrationCardDataApi MigrationCardDataApi => GenerateClient<IMigrationCardDataApi>();
        public IResidentPassportApi ResidentPassportApi => GenerateClient<IResidentPassportApi>();
        public ICompanyBankApi CompanyBankApi => GenerateClient<ICompanyBankApi>();
        public IBuyerTemplateConnectionApi BuyerTemplateConnectionApi => GenerateClient<IBuyerTemplateConnectionApi>();
        public IUnformalizedDocumentApi UnformalizedDocumentApi => GenerateClient<IUnformalizedDocumentApi>();
        public IUserRegulationApi UserRegulationApi => GenerateClient<IUserRegulationApi>();
        public ICalendarApi CalendarApi => GenerateClient<ICalendarApi>();
        public IDiscountSettingsApi DiscountSettingsApi => GenerateClient<IDiscountSettingsApi>();
    }
}