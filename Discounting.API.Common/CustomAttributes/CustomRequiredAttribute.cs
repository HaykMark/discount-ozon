using System;
using System.ComponentModel.DataAnnotations;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class CustomRequiredAttribute : RequiredAttribute
    {
        private readonly AttributeErrorMessage attributeErrorMessage;
        public CustomRequiredAttribute()
        {
            attributeErrorMessage = new AttributeErrorMessage
            {
                AttributeType = AttributeType.Required
            };
        }
        public override string FormatErrorMessage(string name)
        {
            attributeErrorMessage.Message = base.FormatErrorMessage(name);
            return JsonConvert.SerializeObject(attributeErrorMessage, Formatting.Indented);
        }
    }
}