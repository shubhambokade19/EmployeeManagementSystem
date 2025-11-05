namespace Employee.Management.System.Common.Api
{
    public class SearchRequest
    {
        public string? Intent { get; set; } = null;
        public string[]? Fields { get; set; }
        public List<SearchCriterion>? Criterion { get; set; }
        public List<SortSpecification>? Sort { get; set; }
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 0;

        public bool ExistsCriterion(string fieldName, string[] allowedOperators, string searchName = "")
        {
            var criterion = this.Criterion?.FirstOrDefault(o => o?.Field?.Equals(fieldName, StringComparison.OrdinalIgnoreCase) == true && allowedOperators.Contains(o.Operator) && !string.IsNullOrEmpty(o?.Value?.ToString())) ?? null;
            if (criterion == null || string.IsNullOrEmpty(criterion.Field))
            {
                var intentText = string.IsNullOrEmpty(Intent) ? "" : " for search intent '" + Intent + "'";
                throw new ApiException($"{searchName} - Value for '{fieldName}' not found! Please check documentation" + intentText);
            }
            return true;
        }
        public bool ExistsCriterionField(string fieldName)
        {
            var criterion = this.Criterion?.FirstOrDefault(o => o?.Field?.Equals(fieldName, StringComparison.OrdinalIgnoreCase) == true);
            return criterion != null && !string.IsNullOrEmpty(criterion.Field);
        }
        public bool ExistsField(string fieldName)
        {
            var hasField = this.Fields?.Any(f => string.Equals(f, fieldName, StringComparison.OrdinalIgnoreCase) || f == "*") ?? false;
            return hasField;
        }
        public bool ExistsSortField(string fieldName)
        {
            var sort = this.Sort?.Find(o => o.Field.Equals(fieldName, StringComparison.OrdinalIgnoreCase)) ?? null;
            return sort != null && !string.IsNullOrEmpty(sort.Field);
        }
    }
}
