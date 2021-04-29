using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace Discounting.API.Common.Extensions
{
    public class MediaTypeApiVersionReader : IApiVersionReader
    {
        private const string Pattern = @"[.\D+]";

        public void AddParameters(IApiVersionParameterDescriptionContext context)
        {

        }

        public string Read(HttpRequest request)
        {
            var mediaType = request.Headers["Accept"].Single();
            var versionResult = string.Empty;
            var match = Regex.Split(mediaType, Pattern).Where(m => !string.IsNullOrEmpty(m));
            var enumerable = match as string[] ?? match.ToArray();
            if (enumerable.Any())
            {
                versionResult = string.Join('.', enumerable);
            }
            return versionResult;
        }
    }
}
