using Employee.Management.System.Common.Api;
using System.Text;

namespace Employee.Management.System.Common.Helpers
{
    public static class SearchHelper
    {
        public static string GetSqlStatementForSearch(SearchRequest? searchRequest,
            string idField, Dictionary<string, string> outputFields, Dictionary<string, string> criterionFields,
            string fromClause)
        {
            string clause;
            StringBuilder sql = new StringBuilder(4096);

            sql.Append(GetSelectFields(searchRequest, idField, outputFields));
            sql.Append(Environment.NewLine);
            sql.Append(fromClause);

            clause = GetWhereClause(searchRequest, criterionFields);
            if (!string.IsNullOrEmpty(clause))
            {
                sql.Append(clause);
            }

            clause = GetOrderByClause(searchRequest, outputFields);
            if (!string.IsNullOrEmpty(clause))
            {
                sql.Append(clause);
            }

            clause = SearchRequestHelper.GetOffsetFetchClause(searchRequest);
            if (!string.IsNullOrEmpty(clause))
            {
                sql.Append(clause);
            }
            return sql.ToString();
        }
        public static string GetSelectFields(SearchRequest? searchRequest, string idField, Dictionary<string, string> outputFields)
        {
            if (outputFields == null || string.IsNullOrEmpty(idField))
                throw new ArgumentException("Output fields or ID field is null or empty.");

            var fields = searchRequest?.Fields ?? Array.Empty<string>();
            var isAllFields = fields.Length > 0 && fields[0] == "*";

            var selectFields = new StringBuilder($"SELECT {idField}");

            foreach (var (key, value) in outputFields)
            {
                // Check if fields are allowed or if all fields are selected
                if (isAllFields || fields.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        selectFields.Append($"{Environment.NewLine}, {value}");
                    }
                }
            }

            return selectFields.ToString();
        }
        public static string GetWhereClause(SearchRequest? searchRequest, Dictionary<string, string> criterionFields)
        {
            if (searchRequest?.Criterion == null || searchRequest.Criterion.Count == 0)
                return string.Empty;

            var caseInsensitiveFields = new Dictionary<string, string>(criterionFields, StringComparer.OrdinalIgnoreCase);
            var whereClause = new StringBuilder();

            int index = 0;
            foreach (var criterion in searchRequest.Criterion)
            {
                if (criterion.Field != null && caseInsensitiveFields.TryGetValue(criterion.Field, out var mappedField))
                {
                    var criterionValue = criterion.Value as string;
                    var currentCriterion = new SearchCriterion
                    {
                        Field = mappedField,
                        Operator = criterion.Operator,
                        Value = criterion.Value
                    };

                    if (index > 0)
                        whereClause.Append(Environment.NewLine);

                    whereClause.Append(SearchRequestHelper.ConvertCriterionToWhereClause(currentCriterion, index++));
                }
            }

            return whereClause.ToString();
        }
        public static string GetOrderByClause(SearchRequest? searchRequest, Dictionary<string, string> outputFields)
        {
            // This method assumes that the search reuest has been validated before making this call ...
            if (searchRequest?.Sort == null || searchRequest.Sort.Count == 0) return string.Empty;

            // Process each criterion clause
            var orderByClause = $"{Environment.NewLine}order by";
            int index = 0;
            string sortFieldName = string.Empty;

            // Create a case-insensitive dictionary
            outputFields = new Dictionary<string, string>(outputFields, StringComparer.OrdinalIgnoreCase);

            foreach (var sort in searchRequest.Sort)
            {
                if (!sort.ValidateOrder(sort.Order))
                {
                    throw new ApiException($"Invalid Order Clause '{sort?.Order}'. Expected ASC or DESC");
                }
                if (!outputFields[sort.Field].Contains("as", StringComparison.OrdinalIgnoreCase))
                {
                    sortFieldName = outputFields[sort.Field];
                }
                else
                {
                    sortFieldName = sort.Field;
                }
                orderByClause = $@"{orderByClause}{(index++ > 0 ? ", " : "")} {sortFieldName} {sort?.Order?.ToLower()}";
            }
            return orderByClause;
        }
        public static string GetWhereClause(List<SearchCriterion> criterionList)
        {
            // This method assumes that the search reuest has been validated before making this call ...
            string whereClause = string.Empty;

            if (criterionList == null || criterionList.Count == 0) return whereClause;

            // Process criterion ...
            int index = 0;
            foreach (var criterion in criterionList)
            {
                whereClause = $@"{whereClause}{Environment.NewLine}{SearchRequestHelper.ConvertCriterionToWhereClause(criterion, index++)}";
            }
            return whereClause;
        }
    }
}
