
using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    /// <summary>
    /// Thrown if the user is not logged in.
    /// </summary>
    /// <remarks>
    /// The web-community consensus is to use the 401 status for unauthenticated users,
    /// despite the status code being originally intended for authorization.
    /// <see href="https://stackoverflow.com/q/3297048/2477619" />
    /// </remarks>
    public class UnauthenticatedException : HttpException
    {
        private const string ErrorMessage = "Missing authorization to perform this action";

        public UnauthenticatedException() : base(
            StatusCodes.Status401Unauthorized,
            new UnauthenticatedErrorDetails(ErrorMessage),
            ErrorMessage
        )
        {
        }
    }

    public class UnauthenticatedErrorDetails : ErrorDetails
    {
        public UnauthenticatedErrorDetails(string message)
        {
            Key = "unauthenticated";
            args = message;
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