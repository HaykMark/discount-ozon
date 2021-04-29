using System;
using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    /// <summary>
    /// Thrown if an object could not be found.
    /// </summary>
    public class NotFoundException : HttpException
    {
        public NotFoundException()
            : base(StatusCodes.Status404NotFound, new NotFoundErrorDetails((string)null))
        {
        }

        public NotFoundException(Type type)
            : base(
                StatusCodes.Status404NotFound,
                new NotFoundErrorDetails(type),
                $"Could not find {type}",
                type.ToString()
            )
        {
        }
        
        public NotFoundException(string name)
            : base(
                StatusCodes.Status404NotFound,
                new NotFoundErrorDetails(name),
                $"Could not find {name}",
                name
            )
        {
        }
    }

    public class NotFoundErrorDetails : ErrorDetails
    {
        public NotFoundErrorDetails(Type fieldType = null)
        {
            Key = "not-found";
            args = fieldType == null ? null : fieldType.ToString();
        }
        
        public NotFoundErrorDetails(string fieldType = null)
        {
            Key = "not-found";
            args = fieldType;
        }

        private string args;
        public override string Key { get; }

        public override string Args
        {
            get => args;
            set => args = value;
        }
    }
}