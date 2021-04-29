
using Discounting.Common.Validation.Errors;
using Discounting.Common.Types;

namespace Discounting.Common.Exceptions
{
    public class BadRequestException : HttpException
    {
        public BadRequestException(string key, string message = null)
            : base(StatusCodes.Status400BadRequest, new BadRequestErrorDetails(key, message), message)
        {
        }
    }

    public class BadRequestErrorDetails : ErrorDetails
    {
        public BadRequestErrorDetails(string message)
        {
            Key = "bad-request";
            args = message;
        }

        public BadRequestErrorDetails(string key, string message)
        {
            Key = key;
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