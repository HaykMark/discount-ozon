using Discounting.Common.Types;

namespace Discounting.Common.Validation.Errors.Attribute.FileType
{
    public class FileTypeErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.FileType;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            return new FileTypeDetails();
        }
    }
}