using Discounting.API.Common.CustomAttributes;
using Newtonsoft.Json;

namespace Discounting.API.Common.ViewModels.Account
{
    /// <summary>
    /// Data sent over the network for JSON web tokens.
    /// </summary>
    public class TokenDTO
    {
        [CustomRequired]
        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }

        [CustomRequired]
        [JsonProperty(PropertyName = "refreshToken")]
        public string RefreshToken { get; set; }
    }
}