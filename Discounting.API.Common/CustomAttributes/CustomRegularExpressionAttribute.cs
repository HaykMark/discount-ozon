using System.ComponentModel.DataAnnotations;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    public class CustomRegularExpressionAttribute : RegularExpressionAttribute
    {
        public CustomRegularExpressionAttribute(string pattern) : base(pattern)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.RegularExpression,
                Message = $"Invalid {name}"
            });
        }
    }
}