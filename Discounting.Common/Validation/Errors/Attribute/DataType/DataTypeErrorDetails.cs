namespace Discounting.Common.Validation.Errors.Attribute.DataType
{
    public class DataTypeErrorDetails : ErrorDetails
    {
        public DataTypeErrorDetails(string dataType)
        {
            Key = "wrong-data-type";
            args = dataType;
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