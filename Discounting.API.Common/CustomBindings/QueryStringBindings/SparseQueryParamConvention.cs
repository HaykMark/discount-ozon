using Discounting.API.Common.CustomAttributes;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Discounting.API.Common.CustomBindings.QueryStringBindings
{
    /// <inheritdoc />
    public class SparseQueryParamConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            //Add single SeparatedQueryStringAttribute object to be used while looping through each parameter.
            SeparatedQueryStringAttribute attribute = null;
            foreach (var parameter in action.Parameters)
            {
                if (attribute == null)
                {
                    attribute = new SeparatedQueryStringAttribute(",");
                    parameter.Action.Filters.Add(attribute);
                }

                attribute.AddKey(parameter.ParameterName);
            }
        }
    }
}