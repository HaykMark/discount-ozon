namespace Discounting.Common.Validation.Errors
{
    public class DateInFutureErrorDetails : ErrorDetails
    {
        public DateInFutureErrorDetails()
        {
            Key = "date-in-future";
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