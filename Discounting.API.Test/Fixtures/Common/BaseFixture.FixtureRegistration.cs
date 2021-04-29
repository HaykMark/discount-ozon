using Discounting.Tests.Fixtures.CompanyAggregates;

namespace Discounting.Tests.Fixtures.Common
{
    public partial class BaseFixture
    {
        public RoleFixture RoleFixture => new RoleFixture(AppState);

        public UserFixture UserFixture => new UserFixture(AppState);

        public AccountFixture AccountFixture => new AccountFixture(AppState);

        public CompanyFixture CompanyFixture => new CompanyFixture(AppState);

        public SupplyFixture SupplyFixture => new SupplyFixture(AppState);

        public FactoringAgreementFixture FactoringAgreementFixture => new FactoringAgreementFixture(AppState);

        public RegistryFixture RegistryFixture => new RegistryFixture(AppState);

        public TariffFixture TariffFixture => new TariffFixture(AppState);

        public DiscountFixture DiscountFixture => new DiscountFixture(AppState);

        public PasswordPolicyFixture PasswordPolicyFixture => new PasswordPolicyFixture(AppState);

        public RegulationFixture RegulationFixture => new RegulationFixture(AppState);
        public UnformalizedDocumentFixture UnformalizedDocumentFixture => new UnformalizedDocumentFixture(AppState);

        public BuyerTemplateConnectionFixture BuyerTemplateConnectionFixture =>
            new BuyerTemplateConnectionFixture(AppState);

        public UserRegulationFixture UserRegulationFixture => new UserRegulationFixture(AppState);
        public CalendarFixture CalendarFixture => new CalendarFixture(AppState);
        public DiscountSettingsFixture DiscountSettingsFixture => new DiscountSettingsFixture(AppState);
    }
}