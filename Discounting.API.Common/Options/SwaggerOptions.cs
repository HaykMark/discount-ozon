using System.IO;
using Microsoft.OpenApi.Models;

namespace Discounting.API.Common.Options
{
    public class SwaggerOptions : OpenApiInfo
    {
        /// <summary>
        /// Initialization of Swagger options, adding url to download postman.zip
        /// </summary>
        public SwaggerOptions()
        {
            Title = "Discounting API";
            Version = "v1";
            Description = "API documentation";
        }
    }
}