using Discounting.Common.Types;

namespace Discounting.Common.Validation.Errors
{
    public interface IErrorFactory
    {
        bool CanHandleType(AttributeType attributeType);
        ErrorDetails GetErrorDetails(string args = null);
    }
}