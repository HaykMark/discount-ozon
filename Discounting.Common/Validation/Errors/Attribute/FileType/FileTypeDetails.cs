namespace Discounting.Common.Validation.Errors.Attribute.FileType
{
    public class FileTypeDetails: ErrorDetails
    {
        public FileTypeDetails(string args = null)
        {
            Key = "wrong-file-type";
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