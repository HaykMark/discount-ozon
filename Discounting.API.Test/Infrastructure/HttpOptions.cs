using System.Net;
using System.Net.Http;

namespace Discounting.Tests.Infrastructure
{
    public class HttpOptions
    {
        public string Uri { get; set; }

        public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

        public object Content { get; set; }

        public HttpStatusCode ExpectedHttpStatusCode { get; set; } = HttpStatusCode.OK;
    }
}