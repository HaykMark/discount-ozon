using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors
{
    public class ReferenceExistsErrorDetails : ErrorDetails
    {
        public ReferenceExistsErrorDetails(string arg1, string arg2)
        {
            Key = "reference-already-exists";
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