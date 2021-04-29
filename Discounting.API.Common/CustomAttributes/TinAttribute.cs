using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TinAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var input = Convert.ToString(value);
            return input != null && (input.Length != 10 || input.Length != 12) && input.All(char.IsNumber);
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.Tin,
                Args = null,
                Message = $@"{name} must be 10 or 12 digits"
            }, Formatting.Indented);
        }
    }
}