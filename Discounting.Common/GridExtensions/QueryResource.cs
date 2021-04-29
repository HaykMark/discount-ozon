﻿using System.Collections.Generic;

 namespace Discounting.Common.GridExtensions
{
    public class QueryColumn
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
    }

    public class KeyValuePairResource
    {
        public string Key { get; set; }
        public object[] Values { get; set; }
    }
    
    public class QueryResultDTO<T>
    {
        public int TotalItems { get; set; }
        public List<KeyValuePairResource> MultiSelectColumns { set; get; }
        public IEnumerable<T> Items { get; set; }
    }

}
