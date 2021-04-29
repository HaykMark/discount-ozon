namespace Discounting.Common.Validation.Errors
{
    public class DbUpdateErrorDetails : ErrorDetails
    {
        public DbUpdateErrorDetails()
        {
            Key = "db-update";
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