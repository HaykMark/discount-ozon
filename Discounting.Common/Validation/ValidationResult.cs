using System.Linq;
using Discounting.Common.Exceptions;

namespace Discounting.Common.Validation
{
    public class ValidationResult 
    {
        public readonly ValidationErrors Errors = new ValidationErrors();

        public ValidationResult()
        {
            
        }
        public bool IsValid
        {
            get { return !Errors.Any() || Errors.All(e => e == null); }
        }

        public void ThrowIfAny()
        {
            if (!IsValid)
            {
                throw new ValidationException(Errors.Where(e => e != null));
            }
        }

        public int ErrorCount
        {
            get { return Errors.Count(e => e != null); }
        }
    }
}