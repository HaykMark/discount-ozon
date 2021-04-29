
using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    /// <summary>
    /// Thrown if the media type is not supported for this operation.
    /// </summary>
    public class UnsupportedMediaTypeException : HttpException
    {
        private const string ErrorMessage = "This media type is not supported";
        public UnsupportedMediaTypeException() : base(
            StatusCodes.Status415UnsupportedMediaType,
            new UnsupportedMediaTypeErrorDetails(ErrorMessage),
            ErrorMessage
        )
        {
        }
    }
    
    public class UnsupportedMediaTypeErrorDetails : ErrorDetails
    {
        public UnsupportedMediaTypeErrorDetails(string message)
        {
            Key = "unsupported-media-type";
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