using Discounting.Common.Types;
using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors.Attribute.DataType
{
    public class DataTypeErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.DataType;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            var dataTypeArgs = JsonConvert.DeserializeObject<System.ComponentModel.DataAnnotations.DataType>(args);
            return new DataTypeErrorDetails(dataTypeArgs.ToString());
        }
    }
}