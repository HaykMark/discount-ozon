using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Discounting.API.Common.CustomBindings.QueryStringBindings
{
    public class SeparatedValueProvider : QueryStringValueProvider
    {
        private readonly HashSet<string> keys;
        private readonly string separator;
        private readonly IQueryCollection values;

        /// <inheritdoc />
        public SeparatedValueProvider(IQueryCollection values, CultureInfo culture)
            : base(null, values, culture)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes in the key, query string values and a separator.
        /// Here it's a comma, but also anything else, if we want to delimit collections differently.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        public SeparatedValueProvider(
            string key,
            IQueryCollection values,
            string separator
        )
            : this(
                new List<string> {key},
                values,
                separator
            )
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes all the keys which will allow us to apply our logic only to
        /// that specific query string 'key' – so only to a specific parameter.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        public SeparatedValueProvider(
            IEnumerable<string> keys,
            IQueryCollection values,
            string separator
        )
            : base(
                BindingSource.Query,
                values,
                CultureInfo.InvariantCulture
            )
        {
            this.keys = new HashSet<string>(keys);
            this.values = values;
            this.separator = separator;
        }

        /// <summary>
        /// If we configured our provider to run for a specific key only (by passing it through the constructor),
        /// and it’s not equal to the current key, then it will run the default (base) behavior.
        /// In other cases, we will try to split the values based on our separator.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override ValueProviderResult GetValue(string key)
        {
            var result = base.GetValue(key);

            if (keys != null && !keys.Contains(key))
            {
                return result;
            }

            if (result != ValueProviderResult.None &&
                result.Values.Any(x => x.IndexOf(separator, StringComparison.OrdinalIgnoreCase) > 0))
            {
                var splitValues = new StringValues(result.Values
                    .SelectMany(x => x.Split(new[] { separator }, StringSplitOptions.None))
                    .ToArray());

                return new ValueProviderResult(splitValues, result.Culture);
            }

            return result;
        }
    }
}