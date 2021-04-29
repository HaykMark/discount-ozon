using System;
using System.Runtime.Serialization;
using Discounting.Common.Validation;
using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    /// <summary>
    /// An exception implementing this interface can be translated
    /// to an http error response.
    /// </summary>
    public interface IHttpException
    {
        /// <summary>
        /// The http status that will be used in case this exception serialized
        /// </summary>
        int HttpStatus { get; }
    }

    [DataContract]
    public class HttpException : Exception, IHttpException
    {
        [DataMember]
        public ValidationErrors Errors { get; set; } = new ValidationErrors();

        public HttpException(int httpStatus)
        {
            HttpStatus = httpStatus;
        }
        
        public HttpException(
            int httpStatus,
            ErrorDetails errorDetails,
            string message = null,
            string field = null,
            Exception innerException = null
        ) : base(message, innerException)
        {
            HttpStatus = httpStatus;
            Errors.Add(new ValidationError(field, errorDetails));
        }

        public int HttpStatus { get; }
    }
}