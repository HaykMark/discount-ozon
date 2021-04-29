using Discounting.Common.Types;

namespace Discounting.Common.Validation.Errors.Attribute.RegularExpression
{
    public class RegExErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) => 
            attributeType == AttributeType.RegularExpression;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            return new RegExErrorDetails();
        }
    }
}