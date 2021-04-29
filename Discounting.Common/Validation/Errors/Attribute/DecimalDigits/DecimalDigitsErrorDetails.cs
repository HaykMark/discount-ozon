namespace Discounting.Common.Validation.Errors.Attribute.DecimalDigits
{
    public class DecimalDigitsErrorDetails : ErrorDetails
    {
        public DecimalDigitsErrorDetails(string digits)
        {
            Key = "wrong-decimal-digits";
            args = digits;
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