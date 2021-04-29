using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Common.Validation.Errors;
using Discounting.Common.Validation.Errors.Attribute;
using Discounting.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;

namespace Discounting.API.Common.Extensions
{
    public static class ValidationExceptionExtensions
    {
        /// <summary>
        /// This method will initialize the validation exception with
        /// the errors contained in the model state. For every error in the
        /// model state an error field is added to the exception that will
        /// be included in the request response later.
        /// </summary>
        public static ValidationException FromModelState(ModelStateDictionary modelState)
        {
            var errors = new List<ValidationError>();
            foreach (var entry in modelState)
            {
                foreach (var modelError in entry.Value.Errors)
                {
                    // if no further information exists about this error, we simply
                    // use "bad input" to generically describe the error
                    var message = string.IsNullOrEmpty(modelError.ErrorMessage)
                        ? "bad input"
                        : modelError.ErrorMessage;
                    if (message.IsValidJson()) //If true than this is Error-Key json formatted message
                    {
                        var errorMessageJson = JsonConvert.DeserializeObject<AttributeErrorMessage>(message);
                        var errorMessageText = errorMessageJson.Message;

                        var platform = Environment.OSVersion.Platform.ToString();
                        var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform)
                            .Where(r => r.Name != "Microsoft.AspNetCore.Mvc.Razor.Extensions");

                        var types = runtimeAssemblyNames
                            .Select(Assembly.Load)
                            .SelectMany(a => a.ExportedTypes)
                            .Where(t => typeof(IErrorFactory).IsAssignableFrom(t)
                                        && !t.IsInterface);
                        foreach (var t in types)
                        {
                            var factory = (IErrorFactory) Activator.CreateInstance(t);
                            if (factory.CanHandleType(errorMessageJson.AttributeType))
                            {
                                errors.Add(new ValidationError(entry.Key,
                                    errorMessageText,
                                    factory.GetErrorDetails(errorMessageJson.Args?.ToString()))
                                );
                                break;
                            }
                        }
                    }
                    else
                    {
                        errors.Add(string.IsNullOrEmpty(entry.Key)
                            ? new ValidationError(message)
                            : new ValidationError(entry.Key, message));
                    }
                }
            }

            var validationException = new ValidationException(errors);

            return validationException;
        }
    }
}