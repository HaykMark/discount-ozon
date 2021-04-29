using Discounting.Common.Types;
using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors.Attribute.StringLength
{
    public class StringLengthErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.StringLength;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            var stringLengthArgs = JsonConvert.DeserializeObject<RangeAttributeArgs<int>>(args);
            var argsJson = JsonConvert.SerializeObject(new
            {
                LOWER_LEVEL = stringLengthArgs.Min,
                HIGHER_LEVEL = stringLengthArgs.Max
            });
            return new StringLengthErrorDetails(argsJson);
        }
    }
}