using Discounting.Common.GridExtensions;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Common.GridExtensions
{
    [ModelBinder(typeof(GridQueryObjectModelBinder<QueryObjectDTO>))]
    public class QueryObjectDTO
    {
        public KeyValuePairResource[] Filters { get; set; }
        public string[] MultiSelectColumns { set; get; }
        public string SortBy { get; set; }
        public bool? IsSortAscending { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    [ModelBinder(typeof(GridQueryObjectModelBinder<ExcelExportQueryObject>))]
    public class ExcelExportQueryObject : QueryObjectDTO
    {
        public string Title { get; set; }
        public QueryColumn[] Columns { get; set; }
    }
}
