using Discounting.Common.Validation.Errors;
using Newtonsoft.Json;

namespace Discounting.Common.Exceptions
{
    /// <summary>
    /// Thrown if the provided arguments doesn't match with the one contained in the route
    /// </summary>
    public class RouteArgumentMismatchException : HttpException
    {
        public RouteArgumentMismatchException(string key, object arg1, object arg2)
            : base(
                StatusCodes.Status409Conflict,
                new RouteArgumentMismatchErrorDetails(key, arg1, arg2),
                $"The provided '{key}' '{arg1}' does not match with '{arg2}' contained in the route.")
        {
        }
    }

    public class RouteArgumentMismatchErrorDetails : ErrorDetails
    {
        public RouteArgumentMismatchErrorDetails(string key, object arg1, object arg2)
        {
            Key = "route-argument-mismatch";
            args = JsonConvert.SerializeObject(new
            {
                KEY = key,
                ARG1 = arg1,
                ARG2 = arg2
            });
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