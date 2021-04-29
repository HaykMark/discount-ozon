using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors
{
    public class GeneralErrorDetails : ErrorDetails
    {
        public GeneralErrorDetails(string key)
        {
            Key = key;
            args = null;
        }

        public GeneralErrorDetails(string key, object args)
        {
            Key = key;
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