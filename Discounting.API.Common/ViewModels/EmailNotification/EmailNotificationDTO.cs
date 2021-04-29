using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;

namespace Discounting.API.Common.ViewModels.EmailNotification
{
    public class EmailNotificationDTO : DTO<Guid>
    {
        [CustomRequired]
        public string ReturnUrl { get; set; }
    }
}