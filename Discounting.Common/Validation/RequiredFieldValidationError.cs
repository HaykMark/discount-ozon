using Discounting.Common.Validation.Errors.Required;

namespace Discounting.Common.Validation
{
    public class RequiredFieldValidationError : ValidationError
    {
        public RequiredFieldValidationError(string field, object args = null)
            : base(field, "Field is required", new RequiredErrorDetails(args))
        { }
    }
}