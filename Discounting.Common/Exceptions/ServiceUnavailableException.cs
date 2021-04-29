
using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    public class ServiceUnavailableException : HttpException
    {
        public ServiceUnavailableException(string message = null)
            : base(StatusCodes.Status503ServiceUnavailable, new ServiceUnavailableErrorDetails(message), message)
        {
        }
    }

    public class ServiceUnavailableErrorDetails : ErrorDetails
    {
        public ServiceUnavailableErrorDetails(string message)
        {
            Key = "service-unavailable";
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