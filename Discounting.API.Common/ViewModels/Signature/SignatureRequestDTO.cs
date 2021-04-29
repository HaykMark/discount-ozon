using System;
using Discounting.API.Common.CustomAttributes;
using Microsoft.AspNetCore.Http;

namespace Discounting.API.Common.ViewModels.Signature
{
    public class SignatureRequestDTO
    {
        [CustomRequired]
        public Guid SourceId { get; set; }
        [CustomRequired]
        public IFormFile File { get; set; }
        public string FileName { get; set; }
        [CustomRequired]
        [CustomStringLength(255, MinimumLength = 3)]
        public string OriginalName { get; set; }
    }
}