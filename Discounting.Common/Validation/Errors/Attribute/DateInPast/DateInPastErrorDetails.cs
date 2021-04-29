namespace Discounting.Common.Validation.Errors.Attribute.DateInPast
{
    public class DateInPastErrorDetails : ErrorDetails
    {
        public DateInPastErrorDetails()
        {
            Key = "date-in-past";
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