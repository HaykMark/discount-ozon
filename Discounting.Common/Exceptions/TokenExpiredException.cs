using System;
using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    /// <summary>
    /// Typically thrown by JWT validation handler when access_toke is expired.
    /// </summary>
    public class TokenExpiredException : HttpException
    {
        private const string ErrorMessage = "Token has expired";
        public TokenExpiredException()
            : base(StatusCodes.Status401Unauthorized, new TokenExpiredErrorDetails(ErrorMessage), ErrorMessage)
        {
        }
    }

    public class TokenExpiredErrorDetails : ErrorDetails
    {
        public TokenExpiredErrorDetails(string message)
        {
            Key = "token-expired";
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