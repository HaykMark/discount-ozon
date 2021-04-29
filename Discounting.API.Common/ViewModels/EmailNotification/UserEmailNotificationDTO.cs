using System;
using Discounting.API.Common.Email;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.EmailNotification
{
    public class UserEmailNotificationDTO : DTO<Guid>
    {
        public UserEmailEventType Type { get; set; }
    }
}