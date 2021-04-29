using System;
using Microsoft.AspNetCore.Http;

namespace Discounting.Logics.Models
{
    public class SignatureRequest
    {
        public Guid SourceId { get; set; }
        public IFormFile File { get; set; }
        public string FileName { get; set; }
        public string OriginalName { get; set; }
    }
}