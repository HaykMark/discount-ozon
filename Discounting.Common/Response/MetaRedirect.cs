namespace Discounting.Common.Response
{
    /// <summary>
    /// JSON object for meta property redirects: will force the client to navigate to a certain url
    /// </summary>
    public class MetaRedirect
    {
        public string url { get; set; }
    }
}