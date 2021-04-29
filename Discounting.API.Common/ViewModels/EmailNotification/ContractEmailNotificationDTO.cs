using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.Email;

namespace Discounting.API.Common.ViewModels.EmailNotification
{
    public class ContractEmailNotificationDTO : EmailNotificationDTO
    {
        [CustomRequired]
        public ContractEmailEventType Type { get; set; }
    }
}