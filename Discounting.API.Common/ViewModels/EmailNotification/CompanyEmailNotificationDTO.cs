using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.Email;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.EmailNotification
{
    public class CompanyEmailNotificationDTO : DTO<Guid>
    {
        public CompanyEmailEventType Type { get; set; }
    }
}