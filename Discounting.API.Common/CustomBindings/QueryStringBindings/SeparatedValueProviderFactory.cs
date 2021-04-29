using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Discounting.API.Common.CustomBindings.QueryStringBindings
{
    /// <summary>
    /// Handles the cases where we want to pass a specific keys
    /// Apply the value provider to a specific parameter only, or not.
    /// </summary>
    public class SeparatedValueProviderFactory : IValueProviderFactory
    {
        private readonly string separator;
        private HashSet<string> keys;

        public SeparatedValueProviderFactory(string separator) 
            : this((IEnumerable<string>)null, separator)
        { }

        public SeparatedValueProviderFactory(string key, string separator) 
            : this(new List<string> { key }, separator)
        {
        }

        public SeparatedValueProviderFactory(IEnumerable<string> keys, string separator)
        {
            this.keys = keys != null ? new HashSet<string>(keys) : null;
            this.separator = separator;
        }

        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            context.ValueProviders.Insert(0,
                new SeparatedValueProvider(keys, context.ActionContext.HttpContext.Request.Query,
                    separator));
            return Task.CompletedTask;
        }

        public void AddKey(string key)
        {
            if (keys == null)
            {
                keys = new HashSet<string>();
            }

            keys.Add(key);
        }
    }
}