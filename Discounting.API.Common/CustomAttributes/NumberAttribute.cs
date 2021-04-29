using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var input = Convert.ToString(value);
            return input == null || input.All(char.IsNumber);
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.Number,
                Args = null,
                Message = $@"{name} should contain only numbers"
            }, Formatting.Indented);
        }
    }
}