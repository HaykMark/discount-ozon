using System;
using System.Collections.Generic;
using System.Linq;
using Discounting.Common.Types;
using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors.Attribute.WhiteList
{
    public class WhiteListErrorDetailsFactory : IErrorFactory
    {
        public bool CanHandleType(AttributeType attributeType) =>
            attributeType == AttributeType.WhiteList;

        public ErrorDetails GetErrorDetails(string args = null)
        {
            var whiteListArgs =
                JsonConvert.DeserializeObject<HashSet<IComparable>>(args);
            return new WhiteListErrorDetails(JsonConvert.SerializeObject(whiteListArgs.ToList()));
        }
    }
}