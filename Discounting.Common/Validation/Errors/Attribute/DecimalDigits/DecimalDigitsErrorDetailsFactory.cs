using Discounting.Common.Types;
using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors.Attribute.DecimalDigits
{
    public class DecimalDigitsErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.DecimalDigits;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            var decimalDigitsArgs = JsonConvert.DeserializeObject<string>(args);
            return new DecimalDigitsErrorDetails(decimalDigitsArgs);
        }
    }
}