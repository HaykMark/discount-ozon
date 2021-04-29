using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.Email;

namespace Discounting.API.Common.ViewModels.EmailNotification
{
    public class RegistryEmailNotificationDTO : EmailNotificationDTO
    {
        //CompanyId of the user that performed the action(sent, signed and declined)
        [CustomRequired]
        public Guid CompanyId { get; set; }
        [CustomRequired]
        public RegistryEmailEventType Type { get; set; }
    }
}