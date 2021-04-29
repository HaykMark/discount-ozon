using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors
{
    public class NotRelatedErrorDetails : ErrorDetails
    {
        public NotRelatedErrorDetails(string arg1, string arg2)
        {
            Key = "not-related";
            args = JsonConvert.SerializeObject(new
            {
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