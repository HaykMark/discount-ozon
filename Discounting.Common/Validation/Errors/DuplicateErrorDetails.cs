namespace Discounting.Common.Validation.Errors
{
    public class DuplicateErrorDetails : ErrorDetails
    {
        public DuplicateErrorDetails()
        {
            Key = "duplicate";
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