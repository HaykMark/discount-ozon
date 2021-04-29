using System;
using Discounting.API.Common.Constants;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Common.Types;
using Discounting.Entities;

namespace Discounting.API.Common.ViewModels
{
    /// <summary>
    /// Meta data for an uploaded file
    /// </summary>
    public class UploadDTO : DTO<Guid>
    {
        [CustomRequired]
        [CustomStringLength(1024, MinimumLength = 1)]
        public string Name { get; set; }
        public long Size { get; set; }
        [CustomStringLength(255, MinimumLength = 3)]
        public string ContentType { get; set; }
        public UploadProvider Provider { get; set; }
        public Guid ProviderId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public Guid UserId { get; set; }
        /// <summary>
        /// The route to the endpoint that returns the upload as filestream
        /// </summary>
        public Location location => new Location {
            pathname = $@"/{Routes.Uploads}/{Routes.FileSubRoute}"
                .Replace(Routes.DetailSubRoute, Id.ToString())
        };
    }
}