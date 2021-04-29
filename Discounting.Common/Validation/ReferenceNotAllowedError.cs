using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Validation
{
    public class ReferenceNotAllowedError : ValidationError
    {
        public ReferenceNotAllowedError(string field)
            : base(field, "Referencing this object is not allowed",
                new GeneralErrorDetails("referencing-not-allowed"))
        {
        }

        public ReferenceNotAllowedError(string field, string message, string key)
            : base(field, message, new GeneralErrorDetails(key))
        {
        }
        
        public ReferenceNotAllowedError(string field, string message, ErrorDetails errorDetails)
            : base(field, message, errorDetails)
        {
        }
    }
}