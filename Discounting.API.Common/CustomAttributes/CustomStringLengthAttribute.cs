using System;
using System.ComponentModel.DataAnnotations;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class CustomStringLengthAttribute : StringLengthAttribute
    {
        public CustomStringLengthAttribute(int maximumLength) : base(maximumLength)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.StringLength,
                Message = base.FormatErrorMessage(name),
                Args = new RangeAttributeArgs<int>
                {
                    Max = MaximumLength,
                    Min = MinimumLength
                }
            }, Formatting.Indented);
        }
    }
}