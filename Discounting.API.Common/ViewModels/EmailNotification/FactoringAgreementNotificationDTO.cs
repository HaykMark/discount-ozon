using Discounting.API.Common.Email;

namespace Discounting.API.Common.ViewModels.EmailNotification
{
    public class FactoringAgreementNotificationDTO : EmailNotificationDTO
    {
        public FactoringAgreementEmailEventType Type { get; set; }
    }
}