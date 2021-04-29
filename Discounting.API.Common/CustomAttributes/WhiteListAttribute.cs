using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Discounting.Common.Types;
using Discounting.Common.Validation.Errors.Attribute;
using Discounting.Helpers;
using Newtonsoft.Json;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class WhiteListAttribute : ValidationAttribute
    {
        private HashSet<IConvertible> WhiteList { get; }

        public WhiteListAttribute(params short[] whiteList)
        {
            WhiteList = new HashSet<IConvertible>();

            foreach (var val in whiteList)
            {
                WhiteList.Add(val);
            }
        }

        public WhiteListAttribute(params string[] whiteList)
        {
            WhiteList = new HashSet<IConvertible>();

            foreach (var val in whiteList)
            {
                WhiteList.Add(val);
            }
        }

        public WhiteListAttribute(params int[] whiteList)
        {
            WhiteList = new HashSet<IConvertible>();

            foreach (var val in whiteList)
            {
                WhiteList.Add(val);
            }
        }

        public WhiteListAttribute(params double[] whiteList)
        {
            WhiteList = new HashSet<IConvertible>();

            foreach (var val in whiteList)
            {
                WhiteList.Add(val);
            }
        }

        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            return WhiteList.Contains(Convert.ChangeType(value, WhiteList.First().GetType()));
        }
 
        /// <inheritdoc />
        public override string FormatErrorMessage(string name)
        {
            return JsonConvert.SerializeObject(new AttributeErrorMessage
            {
                AttributeType = AttributeType.WhiteList,
                Args = WhiteList,
                Message = $@"{name} must have one of these values:
                          {string.Join(",", WhiteList)}
                          ".AsOneLine()
            }, Formatting.Indented);
        }
    }
}