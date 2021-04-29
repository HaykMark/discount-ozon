using System.ComponentModel.DataAnnotations;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    public class CustomDataTypeAttribute : DataTypeAttribute
    {
        public CustomDataTypeAttribute(DataType dataType) : base(dataType)
        {
        }

        public CustomDataTypeAttribute(string customDataType) : base(customDataType)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.DataType,
                Message = base.FormatErrorMessage(name),
                Args = DataType
            }, Formatting.Indented);
        }
    }
}