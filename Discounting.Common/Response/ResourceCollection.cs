using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discounting.Common.Response
{
    /// <summary>
    /// Model for collection response body. 
    /// </summary>
    [JsonObject]
    public class ResourceCollection<T> : List<T>
    {
        public ResourceCollection() : base()
        { }
        public ResourceCollection(IEnumerable<T> collection) : base(collection)
        { }

        public ResourceCollection(IEnumerable<T> collection, int maxLength) : base(collection)
        {
            MaxLength = maxLength;
        }

        public IEnumerable<T> Items { get => ToArray(); set => AddRange(value); }
        public int MaxLength { get; }
    }
}