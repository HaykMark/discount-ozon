using System;
using Discounting.Common.Validation.Errors;
using Microsoft.AspNetCore.Http;

namespace Discounting.Common.Exceptions
{
    /// <summary>
    /// Typically thrown by exception middleware if the 
    /// caught exception cannot be further interpreted.
    /// </summary>
    public class UnknownException : HttpException
    {
        public UnknownException(string message, Exception innerException)
            : base(
                StatusCodes.Status500InternalServerError,
                new UnknownErrorDetails(message),
                message,
                "",
                innerException
            )
        {
        }
    }

    public class UnknownErrorDetails : ErrorDetails
    {
        public UnknownErrorDetails(string message)
        {
            Key = "unknown";
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