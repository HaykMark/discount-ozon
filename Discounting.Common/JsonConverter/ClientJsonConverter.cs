using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;

namespace Discounting.Common.JsonConverter
{
    public class ClientJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(
                "Unnecessary because CanWrite is false. The type will skip the converter.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            var token = jsonObject.SelectToken(
                objectType.GetInterface(nameof(IEnumerable)) != null &&
                !objectType.FullName.Contains("ResourceCollection")
                    ? "data.items"
                    : "data");

            if (token == null)
            {
                var target = jsonObject.ToObject(objectType);

                return target;
            }

            var data = token.ToObject(objectType);

            return data;
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsValueType || !objectType.IsPrimitive && objectType.Namespace != nameof(System);
        }
    }
}
