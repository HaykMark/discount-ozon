using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors.Required
{
    public class RequiredErrorDetails : ErrorDetails
    {
        public RequiredErrorDetails(object args = null)
        {
            Key = "required";
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