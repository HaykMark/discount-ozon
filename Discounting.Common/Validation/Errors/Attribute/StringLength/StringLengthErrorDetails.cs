namespace Discounting.Common.Validation.Errors.Attribute.StringLength
{
    public class StringLengthErrorDetails : ErrorDetails
    {
        public StringLengthErrorDetails(string range)
        {
            Key = "wrong-string-length";
            args = range;
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