using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Validation
{
    public class NotFoundValidationError : ValidationError
    {
        public NotFoundValidationError(string field)
            : base(field, "Object does not exist", new GeneralErrorDetails("not-found"))
        { }
    }
}