namespace Discounting.Common.Validation.Errors.Attribute.Tin
{
    public class TinErrorDetails : ErrorDetails
    {
        public TinErrorDetails(string args = null)
        {
            Key = "wrong-tin";
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