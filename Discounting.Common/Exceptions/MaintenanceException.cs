using Discounting.Common.Validation.Errors;

namespace Discounting.Common.Exceptions
{
    public class MaintenanceException : HttpException
    {
        private const string ErrorMessage = "The server is currently undergoing maintenance";
        public MaintenanceException() 
            : base(StatusCodes.Status503ServiceUnavailable, new MaintenanceErrorDetails(ErrorMessage), ErrorMessage)
        {
        }

        public class MaintenanceErrorDetails : ErrorDetails
        {
            public MaintenanceErrorDetails(string message)
            {
                Key = "maintenance";
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
}