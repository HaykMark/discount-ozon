using System;
using System.ComponentModel.DataAnnotations;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    public sealed class CustomEmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var input = Convert.ToString(value);
            if (input.Contains('@'))
            {
                var email = new EmailAddressAttribute();
                return email.IsValid(value) && input.Length > 4 && input.Length < 50;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.EmailOrUsername,
                Message = "The email is not valid."
            }, Formatting.Indented);
        }
    }
}