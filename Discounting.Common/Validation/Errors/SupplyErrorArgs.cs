using System;

namespace Discounting.Common.Validation.Errors
{
    public class SupplyErrorArgs
    {
        public string DOCUMENT_NUMBER { get; set; }
        public DateTime DOCUMENT_DATE { get; set; }
        public object DOCUMENT_TYPE { get; set; }
    }
}