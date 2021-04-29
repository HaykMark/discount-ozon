using System;
using System.ComponentModel.DataAnnotations;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Discounting.Extensions;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    public class DecimalDigitAttribute : ValidationAttribute
    {
        private readonly int digits;

        public DecimalDigitAttribute(int digits)
        {
            this.digits = digits;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value.GetType() != typeof(decimal))
            {
                throw new ArgumentException("Invalid type: the argument is not of type decimal.");
            }

            var val = (decimal) value;
            return val.HasMaxDigits(digits);
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(
                new AttributeErrorMessage
                {
                    AttributeType = AttributeType.DecimalDigits,
                    Message = $"Invalid Target {name}; Maximum {digits} Decimal Points.",
                    Args = digits
                }, Formatting.Indented);
        }
    }
}