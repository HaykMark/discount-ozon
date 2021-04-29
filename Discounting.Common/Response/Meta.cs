using System.Collections.Generic;

namespace Discounting.Common.Response
{
    /// <summary>
    /// JSON object for the response payload, contains a "meta" property with instructions
    /// </summary>
    public class Meta
    {
        public List<IMetaNotification> notification { get; set; }
        public MetaRedirect redirect { get; set; } =  new MetaRedirect();
    }
}