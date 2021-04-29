using Discounting.Common.Types;

namespace Discounting.Common.Validation.Errors.Required
{
    public class RequiredErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.Required;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            return new RequiredErrorDetails();
        }
    }
}