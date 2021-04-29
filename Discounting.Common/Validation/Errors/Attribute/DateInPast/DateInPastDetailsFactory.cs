using Discounting.Common.Types;

namespace Discounting.Common.Validation.Errors.Attribute.DateInPast
{
    public class DateInPastDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.DateInPast;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            return new DateInPastErrorDetails();
        }
    }
}