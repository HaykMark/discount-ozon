namespace Discounting.API.Common.Constants
{
    public class RouteParams
    {
        public const string Id = "id";
    }

    public static class Routes
    {
        public const string DetailSubRoute = "{" + RouteParams.Id + "}";
        public const string FileSubRoute = DetailSubRoute + "/file";
        public const string WithSignatures = DetailSubRoute + "/signatures/";
        public const string SignatureFileSubRoute = WithSignatures + DetailSignatureFileSubRoute + "/file";
        public const string SignaturesFileSubRoute = WithSignatures + DetailSignatureFileSubRoute + "/files";
        public const string DetailSignatureFileSubRoute = "{sid}";
        public static class Account
        {
            public const string Base = "api/user-account";
            public const string Password = Base + "/password";
        }

        public const string Roles = "api/roles";

        public static class User
        {
            public const string List = "api/users";
            public const string Roles = "api/user-roles";
        }

        public const string Audit = "api/audit";
        public const string Calendar = "api/calendar";
        public const string Uploads = "api/uploads";
        public const string Templates = "api/templates";
        public const string CompanyRegulations = "api/company-regulations";
        public const string UserRegulations = "api/user-regulations";

        public const string Companies = "api/companies";
        public const string FactoringAgreements = "api/factoring-agreements";
        public const string SupplyFactoringAgreements = "api/supply-factoring-agreements";
        public const string Supplies = "api/supplies";
        public const string Tariffs = "api/tariffs";
        public const string Discounts = "api/discounts";
        public const string DiscountSettings = "api/discount-settings";
        public const string Contracts = "api/contracts";
        public const string Regulations = "api/regulations";

        public const string Registries = "api/registries";
        public const string BuyerTemplateConnections = "api/buyer-template-connections";
        public const string UnformalizedDocuments = "api/unformalized-documents";
        public const string Signatures = "api/signatures";

    }
}