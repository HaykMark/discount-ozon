using Discounting.Common.Types;

namespace Discounting.Common.Validation.Errors.Attribute.Tin
{
    public class TinErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.Tin;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            return new TinErrorDetails();
        }
    }
}