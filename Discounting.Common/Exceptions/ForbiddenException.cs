using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    /// <summary>
    /// Thrown if user is authenticated but not allowed to access the area
    /// or execute a function
    /// </summary>
    public class ForbiddenException : HttpException
    {
        private const string ErrorMessage = "You do not have sufficient permissions to perform this operation";

        public ForbiddenException()
            : base(StatusCodes.Status403Forbidden, new ForbiddenErrorDetails(), ErrorMessage)
        {
        }

        public ForbiddenException(string key, string message)
            : base(StatusCodes.Status403Forbidden, new ForbiddenErrorDetails(key), message)
        {
        }

        public ForbiddenException(
            string key,
            string message,
            string field
        )
            : base(StatusCodes.Status403Forbidden, new ForbiddenErrorDetails(key), message, field)
        {
        }
        
        public ForbiddenException(
            string key,
            string message,
            string field,
            string args
        )
            : base(StatusCodes.Status403Forbidden, new ForbiddenErrorDetails(key, args), message, field)
        {
        }
    }

    public class ForbiddenErrorDetails : ErrorDetails
    {
        public ForbiddenErrorDetails()
        {
            Key = "forbidden";
            args = null;
        }

        public ForbiddenErrorDetails(string key)
        {
            Key = key;
            args = null;
        }
        
        public ForbiddenErrorDetails(string key, string args)
        {
            Key = key;
            Args = args;
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