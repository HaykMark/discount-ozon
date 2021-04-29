namespace Discounting.Common.Validation.Errors.Range
{
    public class RangeErrorDetails : ErrorDetails
    {
        public RangeErrorDetails(string args)
        {
            Key = "wrong-level";
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