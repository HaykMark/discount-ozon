using System.Collections.Generic;
using Discounting.Common.Validation.Errors;
using Discounting.Helpers;

namespace Discounting.Common.Validation
{
    /// <summary>
    /// A validation error typically refers to a form field and carries
    /// a message informing the frontend about what went wrong.
    /// </summary>
    public class ValidationError
    {
        public string Field { get; }
        public string Message { get; }
        public string Key { get; }
        public string Args { get; }
    
        //For serialization
        public ValidationError()
        { }
        
        public ValidationError(string message)
        {
            Message = message;
        }

        public ValidationError(string field, string message)
        {
            Field = field.FirstCharacterToLower();
            Message = message;
        }
        
        public ValidationError(string field, ErrorDetails errorDetailsMessage)
        {
            Field = field.FirstCharacterToLower();
            Key = errorDetailsMessage.Key;
            Args = errorDetailsMessage.Args;
        }
        //This method is for the Frontend to get the EN error message to build it in the frontend during the development
        //TODO: After Frontend translate all the error messages this method could be deleted
        public ValidationError(string field, string message, ErrorDetails errorDetailsMessage)
        {
            Field = field.FirstCharacterToLower();
            Message = message;
            Key = errorDetailsMessage.Key;
            Args = errorDetailsMessage.Args;
        }
    }

    public class ValidationErrors : List<ValidationError>
    { }

}