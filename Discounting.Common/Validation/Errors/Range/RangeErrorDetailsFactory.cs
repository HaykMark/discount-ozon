using System;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors.Range
{
    public class RangeErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.Range;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            var rangeArgs =
                JsonConvert.DeserializeObject<RangeAttributeArgs<IComparable>>(args);
            var argsJson = JsonConvert.SerializeObject(new
            {
                LOWER_LEVEL = rangeArgs.Min,
                HIGHER_LEVEL = rangeArgs.Max
            });
            return new RangeErrorDetails(argsJson);
        }
    }
}