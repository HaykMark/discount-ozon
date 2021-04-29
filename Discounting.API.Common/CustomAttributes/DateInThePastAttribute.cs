using System;
using System.ComponentModel.DataAnnotations;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DateInThePastAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is null || !(value is DateTime date) || date < DateTime.UtcNow;
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.DateInPast,
                Message = $"{name} must not be in the future."
            }, Formatting.Indented);
        }
    }
}