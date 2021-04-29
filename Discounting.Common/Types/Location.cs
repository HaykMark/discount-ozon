using System;
using System.Collections.Generic;
using System.Text;

namespace Discounting.Common.Types
{
    public class Location
    {
        public string host { get; set; }
        public string pathname { get; set; }
        public string scheme { get; set; }
        public string href { get => $"{scheme}://{host}{pathname}"; }
    }
}
