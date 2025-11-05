namespace Employee.Management.System.Common.Api
{
    public class SearchRequestUtils
    {
        public static string? ExtractAndRemoveCriterion(SearchRequest searchRequest, string fieldName)
        {
            var criterion = searchRequest.Criterion?.Find(o => o?.Field?.Equals(fieldName, StringComparison.OrdinalIgnoreCase) == true);
            if (criterion != null)
            {
                searchRequest.Criterion?.Remove(criterion);
                return criterion?.Value?.ToString();
            }
            return null;
        }
        public static DateTime? ExtractAndRemoveDateCriterion(SearchRequest searchRequest, string fieldName)
        {
            var criterion = searchRequest.Criterion?.Find(o =>
                o?.Field?.Equals(fieldName, StringComparison.OrdinalIgnoreCase) == true);

            if (criterion != null)
            {
                searchRequest.Criterion?.Remove(criterion);

                if (DateTime.TryParse(criterion?.Value?.ToString(), out var parsedDate))
                {
                    return parsedDate;
                }
            }

            return null;
        }
        public static int ExtractAndRemovePage(SearchRequest searchRequest)
        {
            int page = searchRequest.Page;
            searchRequest.Page = 0;
            return page;
        }

        public static int ExtractAndRemovePageSize(SearchRequest searchRequest)
        {
            int pageSize = searchRequest.PageSize;
            searchRequest.PageSize = 0;
            return pageSize;
        }

        public static (string FieldName, string Order)? ExtractAndRemoveSort(SearchRequest searchRequest)
        {
            if (searchRequest.Sort != null && searchRequest.Sort.Count > 0)
            {
                var sort = searchRequest.Sort[0];
                searchRequest.Sort.RemoveAt(0);
                return (sort.Field, sort.Order);
            }
            return null;
        }
    }
}
