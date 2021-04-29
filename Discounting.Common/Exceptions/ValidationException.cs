using System.Collections.Generic;
using Discounting.Common.Validation;
using Discounting.Common.Validation.Errors;
using Microsoft.AspNetCore.Http;

namespace Discounting.Common.Exceptions
{
    public class ValidationException : HttpException
    {
        public ValidationException(string field, string text, ErrorDetails errorDetails) : base(StatusCodes
            .Status422UnprocessableEntity, errorDetails, text, field)
        {
        }

        public ValidationException(IEnumerable<ValidationError> errors) : base(StatusCodes.Status422UnprocessableEntity)
        {
            AddErrors(errors);
        }

        public ValidationException(ValidationError error) : base(StatusCodes.Status422UnprocessableEntity)
        {
            AddError(error);
        }

        private void AddErrors(IEnumerable<ValidationError> errors)
        {
            foreach (var error in errors)
            {
                AddError(error);
            }
        }

        private void AddError(ValidationError error)
        {
            Errors.Add(error);
        }
    }
}