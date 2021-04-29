namespace Discounting.Common.Validation.Errors.Attribute.RegularExpression
{
    public class RegExErrorDetails : ErrorDetails
    {
        public RegExErrorDetails()
        {
            Key = "invalid-regex-pattern";
            args = null;
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