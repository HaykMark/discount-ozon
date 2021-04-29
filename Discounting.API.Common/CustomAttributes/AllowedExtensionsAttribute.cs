using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AllowedExtensionAttribute : ValidationAttribute
    {
        private readonly string[] extensions =
            {".txt", ".sgn", ".jpg", ".png", ".jpeg", ".xlsx", ".xls", ".docx", ".doc", ".pdf", ".ppt", ".pptx"};

        public override bool IsValid(object value)
        {
            return value switch
            {
                IFormFile file => HasValidExtension(file),
                IFormFile[] files => files.All(HasValidExtension),
                _ => true
            };
        }

        private bool HasValidExtension(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            return (extension != null && extensions.Contains(extension.ToLower()));
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.FileType,
                Message = "File type is not valid"
            }, Formatting.Indented);
        }
    }
}