namespace Discounting.Common.Validation.Errors.Attribute.EmailOrUsername
{
    public class EmailOrUsernameErrorDetails : ErrorDetails
    {
        public EmailOrUsernameErrorDetails(string args = null)
        {
            Key = "wrong-email";
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