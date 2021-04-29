using System;
using System.ComponentModel.DataAnnotations;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class CustomRangeAttribute : RangeAttribute
    {
        private readonly AttributeErrorMessage attributeErrorMessage;
        protected CustomRangeAttribute(double minimum, double maximum)
            : base(minimum, maximum)
        {
            attributeErrorMessage = new AttributeErrorMessage
            {
                AttributeType = AttributeType.Range,
                Args = new RangeAttributeArgs<double>
                {
                    Min = minimum,
                    Max = maximum
                }
            };
        }

        public CustomRangeAttribute(int minimum, int maximum)
            : base(minimum, maximum)
        {
            attributeErrorMessage = new AttributeErrorMessage
            {
                AttributeType = AttributeType.Range,
                Args = new RangeAttributeArgs<int>
                {
                    Min = minimum,
                    Max = maximum
                }
            };
        }

        public CustomRangeAttribute(Type type, string minimum, string maximum)
            : base(type, minimum, maximum)
        {
            attributeErrorMessage = new AttributeErrorMessage
            {
                AttributeType = AttributeType.Range,
                Args = new RangeAttributeArgs<string>
                {
                    Min = minimum,
                    Max = maximum
                }
            };
        }

        public override string FormatErrorMessage(string name)
        {
            attributeErrorMessage.Message = base.FormatErrorMessage(name);
            return JsonConvert.SerializeObject(attributeErrorMessage, Formatting.Indented);
        }
    }
}