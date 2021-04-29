using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Validation
{
    public class DisabledFieldValidationError : ValidationError
    {
        public DisabledFieldValidationError(string field)
            : base(field, "Field is disabled", new GeneralErrorDetails("disabled"))
        { }
        
        public DisabledFieldValidationError(string field, string message, string key)
            : base(field, message, new GeneralErrorDetails(key))
        { }
    }
}