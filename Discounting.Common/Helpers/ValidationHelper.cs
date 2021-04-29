using System.Collections.Generic;
using System.Linq;
using Discounting.Common.Validation;

namespace Discounting.Common.Helpers
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates if required field has a value
        /// </summary>
        public static IEnumerable<ValidationError> ValidateRequired(this object obj, params string[] fieldNames)
        {
            return (from name in fieldNames
                where obj.GetType().GetProperty(name)?.GetValue(obj) == null
                select new RequiredFieldValidationError(name));
        }

        /// <summary>
        /// Validates if disabled field has a value
        /// </summary>
        public static IEnumerable<ValidationError> ValidateDisabled(this object obj, params string[] fieldNames)
        {
            return (from name in fieldNames
                where obj.GetType().GetProperty(name)?.GetValue(obj) != null
                select new DisabledFieldValidationError(name));
        }
    }
}