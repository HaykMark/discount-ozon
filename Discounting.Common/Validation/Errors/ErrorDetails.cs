namespace Discounting.Common.Validation.Errors
{
    public abstract class ErrorDetails
    {
        public abstract string Key { get; }
        public abstract string Args { get; set; }
    }
}