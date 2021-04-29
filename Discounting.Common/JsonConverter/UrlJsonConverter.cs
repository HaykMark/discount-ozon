using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Discounting.Common.Types;

namespace Discounting.Common.JsonConverter
{
    public class UrlJsonConverter : Newtonsoft.Json.JsonConverter
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public override bool CanRead => false;

        public UrlJsonConverter(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            if (value is Location location)
            {
                location.host = httpContextAccessor.HttpContext.Request.Host.Value;
                location.scheme = httpContextAccessor.HttpContext.Request.Scheme;
                // note: simply using FromObject works here because all child properties
                // of location are primitive types! for complex child properties we
                // would have to use reflection to handle the parent object.
                // @see https://stackoverflow.com/a/29281107/2477619
                var t = JToken.FromObject(location);
                t.WriteTo(writer);
            }
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            // Unnecessary because CanRead property is false. The type will skip the converter.
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type type)
        {
            return type == typeof(Location);
        }
    }
}
