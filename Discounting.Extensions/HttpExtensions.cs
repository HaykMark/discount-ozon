using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discounting.Extensions
{
    /// <summary>
    ///     Represents extension methods for HttpResponseMessage class.
    /// </summary>
    public static class HttpExtensions
    {
        public static HttpRequestMessage WithContent(this HttpRequestMessage request, object content,
            JsonSerializerSettings jsonSettings)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(content, jsonSettings));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return request;
        }

        public static HttpRequestMessage WithApiVersion(this HttpRequestMessage request, string version)
        {
            return request.WithHeader("Accept", version);
        }

        public static async Task<T> GetContentAsync<T>(this HttpResponseMessage response,
            JsonSerializerSettings jsonSettings)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent, jsonSettings);
        }

        public static HttpRequestMessage WithHeader(this HttpRequestMessage request, string key, string value)
        {
            request.Headers.Add(key, value);

            return request;
        }
    }
}