using Employee.Management.System.Common.Api;

namespace Employee.Management.System.Common.Helpers
{
    public static class SearchRequestHelper
    {
        public static string GetCommaSeperatedFieldList(SearchRequest? searchRequest, string alias = "")
        {
            var fields = searchRequest?.Fields;

            if (fields == null || fields.Contains("*"))
            {
                return string.IsNullOrEmpty(alias) ? "*" : $"{alias}.*";
            }

            return string.Join(", ", fields.Select(field => string.IsNullOrEmpty(alias) ? field : $"{alias}.{field}"));
        }

        public static string GetWhereClause(SearchRequest? searchRequest)
        {
            var whereClause = string.Empty;
            if (searchRequest?.Criterion == null || searchRequest.Criterion.Count == 0)
            {
                return whereClause;
            }

            int index = 0;
            foreach (var criterion in searchRequest.Criterion)
            {
                whereClause += ConvertCriterionToWhereClause(criterion, index++);
            }

            return whereClause;

        }

        public static string ConvertCriterionToWhereClause(SearchCriterion criterion, int index)
        {
            var clause = "";

            switch (criterion.Operator.ToLower())
            {
                case "startswith":
                    if (criterion.Value is string value)
                    {
                        clause = $"{criterion.Field} LIKE \"{value.Replace("\"", "\\\"")}%\"";
                    }
                    else
                    {
                        throw new ApiException($"Value type should be string for 'startswith' operator for field '{criterion.Field}'");
                    }
                    break;

                case "endswith":
                    if (criterion.Value is string)
                    {
                        clause = $"{criterion.Field} LIKE \"%{criterion.Value}\"";
                    }
                    else
                    {
                        throw new ApiException($"Value type should be string for 'endswith' operator for field '{criterion.Field}'");
                    }
                    break;

                case "contains":
                    if (criterion.Value is string)
                    {
                        clause = $"{criterion.Field} LIKE \"%{criterion.Value}%\"";
                    }
                    else
                    {
                        throw new ApiException($"Value type should be string for 'contains' operator for field '{criterion.Field}'");
                    }
                    break;

                case "isnull":
                    clause = $"{criterion.Field} IS NULL";
                    break;

                case "isnotnull":
                    clause = $"{criterion.Field} IS NOT NULL";
                    break;

                case "in":
                    if (criterion.Value is string inValuesStr)
                    {
                        var values = inValuesStr.Split(',');
                        if (values.Length == 0)
                        {
                            throw new ApiException($"Value should be a comma-separated list for 'in' operator for field '{criterion.Field}'");
                        }

                        var inValues = string.Join(", ", values.Select(v => $"\"{v.Trim()}\""));
                        clause = $"{criterion.Field} IN ({inValues})";
                    }
                    else
                    {
                        throw new ApiException($"Value type should be string for 'in' operator for field '{criterion.Field}' and it should be a comma-separated list of values");
                    }
                    break;
                case "between":
                    if (criterion.Value is string betweenValuesStr)
                    {
                        var splitedString = betweenValuesStr.Split(',');
                        if (splitedString.Length != 2)
                        {
                            throw new ApiException($"Value should be a comma-separated list of 2 values (minValue, maxValue) for 'between' operator for field '{criterion.Field}'");
                        }

                        var fromValue = splitedString[0].Trim();
                        var toValue = splitedString[1].Trim();
                        clause = $"{criterion.Field} BETWEEN \"{fromValue}\" AND \"{toValue}\"";
                    }
                    else
                    {
                        throw new ApiException($"Value type should be string for 'between' operator for field '{criterion.Field}' and it should be a comma-separated list of 2 values (minValue, maxValue)");
                    }
                    break;

                case "notstartswith":
                    if (criterion.Value is string)
                    {
                        clause = $"{criterion.Field} NOT LIKE \"{criterion.Value}%\"";
                    }
                    else
                    {
                        throw new ApiException($"Value type should be string for 'notstartswith' operator for field '{criterion.Field}'");
                    }
                    break;
                case "containsorcontains":
                    if (criterion.Value is string containOrValue)
                    {
                        var fieldNames = criterion.Field?.Split("or", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                        if (fieldNames.Length == 0)
                        {
                            throw new ApiException($"Field should contain at least one field for 'containsorcontains' operator for field '{criterion.Field}'");
                        }

                        var likeClauses = fieldNames.Select(fieldName => $"{fieldName.Trim()} LIKE \"%{containOrValue}%\"");
                        clause = $"({string.Join(" OR ", likeClauses)})";
                    }
                    else
                    {
                        throw new ApiException($"Value type should be string for 'containsorcontains' operator for field '{criterion.Field}'");
                    }
                    break;
                default:
                    if (criterion.Value != null)
                    {
                        if (criterion.Value is string)
                        {
                            clause = $"{criterion.Field} {criterion.Operator} \"{criterion.Value}\"";
                        }
                        else if (criterion.Value is DateTime)
                        {
                            var date = Convert.ToDateTime(criterion.Value);
                            clause = $"{criterion.Field} {criterion.Operator} \"{date.Year:D4}-{date.Month:D2}-{date.Day:D2} {date.Hour:D2}:{date.Minute:D2}:{date.Second:D2}\"";
                        }
                        else
                        {
                            clause = $"{criterion.Field} {criterion.Operator} {criterion.Value}";
                        }
                    }
                    else
                    {
                        throw new ApiException($"Appropriate non-null value is expected for '{criterion.Operator}' operator for field '{criterion.Field}'. null value received.");
                    }
                    break;
            }
            return $"{(index > 0 ? " and" : " where")} {clause} ";
        }

        public static string GetOrderByClause(SearchRequest? searchRequest)
        {
            // This method assumes that the search reuest has been validated before making this call ...
            if (searchRequest?.Sort == null || searchRequest.Sort.Count == 0) return string.Empty;

            // Process each criterion clause
            var orderByClause = $"{Environment.NewLine}order by";
            int index = 0;
            foreach (var sort in searchRequest.Sort)
            {
                orderByClause = $@"{orderByClause}{(index++ > 0 ? ", " : "")} {sort.Field} {sort?.Order?.ToLower()}";
            }
            return orderByClause;
        }

        public static void Validate(SearchRequest searchRequest, List<string> allowedIntentList, List<string> allowedSearchFieldList)
        {
            ValidateIntent(searchRequest, allowedIntentList);
            ValidateFields(searchRequest, allowedSearchFieldList);
            ValidateSortSpecification(searchRequest, allowedSearchFieldList);
            ValidatePageSpecification(searchRequest);
        }

        public static void ValidateIntent(SearchRequest searchRequest, List<string> allowedIntentList)
        {
            // If the intent is specified, it must be within the allowed intent list ...
            if (!string.IsNullOrEmpty(searchRequest.Intent))
            {
                if (allowedIntentList == null || allowedIntentList.Count == 0)
                {
                    throw new ApiException("Allowed intent list not specified. A list of allowed intents is expected!");
                }

                if (!allowedIntentList.Contains(searchRequest.Intent, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ApiException($"'{searchRequest.Intent}' is not a supported intent for this search request. Please check the documentation for supported intents.");
                }
            }
        }

        public static void ValidateFields(SearchRequest? searchRequest, List<string> allowedSearchFieldList)
        {
            // Fields MUST be specified if the Intent is NOT specified; fields CAN be specified if Intent is specified ...
            if (string.IsNullOrEmpty(searchRequest?.Intent) && (searchRequest?.Fields == null || searchRequest.Fields.Length == 0))
            {
                throw new ApiException("Search request fields not specified. At least 1 field or '*' is expected!");
            }

            // When '*' is specified in the fields, that should be the ONLY field specification ...
            if (searchRequest?.Fields?.Contains("*") == true && searchRequest.Fields.Length > 1)
            {
                throw new ApiException("When '*' is used as a field, it should be the ONLY field specification.");
            }

            // When field specification is NOT '*', then individual check on fields is required to ensure only supported fields are specified ...
            if (searchRequest?.Fields != null && !searchRequest.Fields.Contains("*"))
            {
                // Check that the fields specified are supported fields ...
                foreach (string field in searchRequest.Fields)
                {
                    IsValidSearchField(field, allowedSearchFieldList);
                }
            }
        }

        private static void IsValidSearchField(string fieldName, List<string> allowedSearchFieldList)
        {
            // Ensure the allowed search field list is not null and contains elements
            if (allowedSearchFieldList == null || !allowedSearchFieldList.Any())
            {
                throw new ApiException("Allowed search fields not specified. A list of allowed fields is expected!");
            }

            // Check if the field name is within the allowed fields list
            if (!allowedSearchFieldList.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
            {
                throw new ApiException($"'{fieldName}' is not a supported field for this search request. Please check the documentation for supported fields.");
            }
        }

        public static void ValidateCriterion(SearchRequest searchRequest, List<string> allowedSearchFieldList)
        {
            // Not specifying Search Criterion is acceptable ...
            if (searchRequest.Criterion == null || searchRequest.Criterion.Count == 0) return;

            // '*' cannot be specified as a field in searchRequest criterion ...
            if (searchRequest.Criterion.Any(o => o.Field == "*"))
                throw new ApiException($"'*' cannot be specified in search criterion. Pl check documentation for supported fields");

            // If search criterion is specified, then ensure that the fields and opertaors are as per allowed list ...
            foreach (SearchCriterion c in searchRequest.Criterion)
            {
                if (string.IsNullOrEmpty(c.Field))
                    throw new ApiException($"Field cannot be blank in search criterion. Pl check documentation for supported fields");

                if (string.IsNullOrEmpty(c.Operator))
                    throw new ApiException($"Operator cannot be blank in search criterion. Pl check documentation for supported operators");

                if (c.Value == null)
                    throw new ApiException($"Value cannot be blank or null in search criterion.");

                IsValidSearchField(c.Field, allowedSearchFieldList);
                IsValidOperator(c.Operator);
            }
        }

        private static void IsValidOperator(string opr)
        {
            if (!SearchCriterion.AllowedOperators.Contains(opr, StringComparer.OrdinalIgnoreCase))
                throw new ApiException($"'{opr}' is not a supported. Pl check documentation for supported operators");
        }

        public static void ValidateSortSpecification(SearchRequest? searchRequest, List<string> allowedSearchFieldList)
        {
            var sortList = searchRequest?.Sort;
            if (sortList == null || !sortList.Any()) return;

            foreach (var s in sortList)
            {
                // Validate each sort specification
                if (s?.Field != null)
                {
                    IsValidSearchField(s.Field, allowedSearchFieldList);
                }

                IsValidSortOrder(s?.Order ?? string.Empty);
            }
        }

        private static void IsValidSortOrder(string sortOrder)
        {
            if ((sortOrder.ToLower() == "asc" || sortOrder.ToLower() == "desc")) return;
            throw new ApiException($"'{sortOrder}' is not a supported. Only 'asc' and 'desc' is supported.");
        }

        public static void ValidatePageSpecification(SearchRequest? searchRequest)
        {
            if (searchRequest?.Page == 0 && searchRequest?.PageSize != 0)
                throw new ApiException($"'When pagesize '{searchRequest?.PageSize}' is non-zero, Page number CANNOT be zero");

            if (searchRequest?.Page != 0 && searchRequest?.PageSize == 0)
                throw new ApiException($"'When page number '{searchRequest?.Page}' is non-zero, Page size CANNOT be zero");

            if (searchRequest?.Page < 0 || searchRequest?.PageSize < 0)
                throw new ApiException($"'Page number '{searchRequest?.Page}' and Page size {searchRequest?.PageSize} CANNOT be < 0");

            if (searchRequest?.PageSize > 0 && searchRequest?.Page <= 0)
                throw new ApiException($"'When page size '{searchRequest?.PageSize}' is > 0, Page number '{searchRequest?.Page}' CANNOT be <= 0");
        }

        public static string GetOffsetFetchClause(SearchRequest? searchRequest)
        {
            if (searchRequest?.Page == 0 && searchRequest?.PageSize == 0) return string.Empty;

            ValidatePageSpecification(searchRequest);

            var offset = (searchRequest?.Page - 1) * searchRequest?.PageSize;
            var rowsToFetch = searchRequest?.PageSize;

            // compose and return offset fetch clause ...
            return $"{Environment.NewLine}limit {offset}, {rowsToFetch}";
        }
    }
}
