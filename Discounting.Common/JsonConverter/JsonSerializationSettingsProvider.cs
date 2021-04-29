using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Discounting.Common.JsonConverter
{
    public class JsonSerializationSettingsProvider
    {
        public static JsonSerializerSettings GetSerializeSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = TypeNameHandling.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTime
            };

            return settings;
        }
    }
}
