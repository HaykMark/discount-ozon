using System;
using Discounting.API.Common.CustomBindings.QueryStringBindings;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Discounting.API.Common.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class SeparatedQueryStringAttribute : Attribute, IResourceFilter
    {
        private readonly SeparatedValueProviderFactory factory;

        public SeparatedQueryStringAttribute() : this(",")
        {
        }

        public SeparatedQueryStringAttribute(string separator)
        {
            factory = new SeparatedValueProviderFactory(separator);
        }

        public SeparatedQueryStringAttribute(string key, string separator)
        {
            factory = new SeparatedValueProviderFactory(key, separator);
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.ValueProviderFactories.Insert(0, factory);
        }
        /// <summary>
        /// Adds additional strings to the keys to be used in SeparatedValueProviderFactory
        /// </summary>
        /// <param name="key"></param>
        public void AddKey(string key)
        {
            factory.AddKey(key);
        }
    }
    
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class CommaSeparatedAttribute : Attribute
    {
    }
}