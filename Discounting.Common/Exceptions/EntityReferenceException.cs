using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    public class EntityReferenceException : HttpException
    {
        public EntityReferenceException(string key, string message = "Cannot delete")
            : base(StatusCodes.Status409Conflict, new EntityReferenceErrorDetails(message, key), message) 
        { }
    }
    
    public class EntityReferenceErrorDetails : ErrorDetails
    {
        public EntityReferenceErrorDetails(string message, string key)
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