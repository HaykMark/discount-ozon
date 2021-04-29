namespace Discounting.API.Common.Response
{
    /// <summary>
    /// The content of a successful response must be wrapped in an instance of this class. 
    /// </summary>
    public class DataResponseBody<T> : ResponseBody
    {
        /// <summary>
        /// Holds the content of a successful response.
        /// </summary>
        /// <remarks>
        /// As speified in <a href="http://jsonapi.org/format/#document-top-level">jsonapi</a>:
        /// <em>A response should contain either a data object or an error object,
        /// but not both. If both data and error are present, the error object takes precedence.</em>
        /// </remarks>
        public T data { get; set; }

        public DataResponseBody(T data)
        {
            this.data = data;
        }
    }
}
