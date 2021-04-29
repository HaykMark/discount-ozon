using Discounting.Common.Types;

namespace Discounting.Common.Validation.Errors.Attribute.EmailOrUsername
{
    public class EmailOrUsernameErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.EmailOrUsername;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            return new EmailOrUsernameErrorDetails();
        }
    }
}