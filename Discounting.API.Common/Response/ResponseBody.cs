using Discounting.Common.Response;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Common.Response
{
    /// <summary>
    /// Wrapper for all responses that are returned from our API.
    /// This resembles <a href="http://jsonapi.org/format/#document-top-level">jsonapi's</a>
    /// toplevel structure.
    /// </summary>
    public abstract class ResponseBody : ActionResult
    {
        /// <summary>
        /// Used to include non-standard meta-information.
        /// This resembles <a href="http://jsonapi.org/format/#document-meta">jsonapi's</a> meta.
        /// </summary>
        public Meta meta { get; set; } = new Meta();
    }
}