using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors
{
    public class InvalidErrorDetails: ErrorDetails
    {
        public InvalidErrorDetails(object args = null)
        {
            Key = "invalid";
            this.args = args == null
                ? null
                : JsonConvert.SerializeObject(args);
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