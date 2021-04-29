using System.Linq;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation.Errors;

namespace Discounting.Logics.Validators
{
    public interface ICommonValidations
    {
        void ValidatePhoneNumber(string phoneNumber);
    }

    public class CommonValidations : ICommonValidations
    {
        public void ValidatePhoneNumber(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber) &&
                !phoneNumber.All(char.IsDigit))
            {
                throw new ValidationException(
                    "Phone",
                    "Invalid Phone number",
                    new InvalidErrorDetails()
                );
            }
        }
    }
}