namespace Discounting.Common.Validation.Errors.Attribute.WhiteList
{
    public class WhiteListErrorDetails : ErrorDetails
    {
        public WhiteListErrorDetails(string args)
        {
            Key = "not-whitelisted-argument";
            this.args = args;
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