namespace Discounting.Common.AccessControl
{
    /// <remarks>
    /// The reason to keep all zones in a central place is:
    /// (1) to have good overview
    /// (2) to be able to verify uniqueness of ids via reflection on startup
    /// </remarks>
    public class Zones
    {
        public const string Audit =                "system.audit";
        public const string Users =                "system.users";
        public const string Regulations =          "system.regulations";
        public const string Roles =                "system.roles";
        public const string UserRoles =            "system.users.roles";
        public const string UserRegulations =      "system.users.regulations";
        public const string CompanyRegulations =   "system.companies.regulations";
        public const string Companies =            "companies";
        public const string CompanySettings =      "companies.settings";
        public const string FactoringAgreement =   "companies.factoringAgreements";
        public const string SupplyFactoringAgreement =   "companies.factoringAgreements.supplies";
        public const string Tariffs =              "companies.tariffs";
        public const string DiscountSettings =     "companies.tariffs.discountSettings";
        public const string Supplies =             "supplies";
        public const string SuppliesInProcess =    "supplies.inProcess";
        public const string SuppliesInFinance =    "supplies.inFinance";
        public const string SuppliesNotAvailable = "supplies.notAvailable";
        public const string Contracts =            "contrats";
        public const string Registries =           "registries";
        public const string Discounts =            "registries.discount";
        public const string RegistriesInProcess =  "registries.inProcess";
        public const string RegistriesFinished =   "registries.finished";
        public const string RegistriesCanceled =   "registries.canceled";
        public const string BuyerTemplateConnections =   "templates.buyerTemplateConnections";
        public const string UnformalizedDocuments =   "unformalizedDocument";
        public const string Calendar =             "calendar";
        public const string Signatures =           "signature";
    }
}