namespace Discounting.API.Common.Email
{
    public enum ContractEmailEventType : byte
    {
        Created,
        Updated
    }

    public enum UnformalizedDocumentEmailEventType : byte
    {
        Signed,
        Sent,
        Declined
    }
    
    public enum RegistryEmailEventType : byte
    {
        Declined,
        SellerSignedDiscount,
        BuyerConfirmedDiscount,
        Signed,
        BuyerSignedDiscount,
        BuyerConfirmedDiscountWithPercentageChange
    }

    public enum CompanyEmailEventType : byte
    {
       Deactivation,
       Activation
    }

    public enum UserEmailEventType : byte
    {
        Deactivation,
        Activation
    }

    public enum FactoringAgreementEmailEventType : byte
    {
        FactoringAgreementAdded,
        FactoringAgreementSupplyAdded,
        FactoringAgreementSupplyConfirmed,
        FactoringAgreementSupplyDeactivated,
        FactoringAgreementSupplyActivated
    }
}