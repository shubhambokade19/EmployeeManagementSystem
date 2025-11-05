using System.Text;

namespace Employee.Management.System.Common.Api
{
    public class WhereClauseBuilder
    {
        private StringBuilder whereClause;

        public string WhereClause => whereClause.ToString();

        public WhereClauseBuilder()
        {
            whereClause = new StringBuilder(4096);
        }

        public void AddClause(string fieldName, SearchCriterion criterion, bool escapeSpecialChars = true)
        {
            if (string.IsNullOrEmpty(whereClause.ToString()))
            {
                whereClause.Append(ConvertCriterionToWhereClause(criterion, fieldName, escapeSpecialChars));
            }
            else
            {
                whereClause.Append(" and " + ConvertCriterionToWhereClause(criterion, fieldName, escapeSpecialChars));
            }
        }

        private string ConvertCriterionToWhereClause(SearchCriterion criterion, string fieldName, bool escapeSpecialChars = true)
        {
            if (criterion == null) throw new ArgumentNullException(nameof(criterion));
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException("Field name cannot be null or empty.", nameof(fieldName));

            var clause = "";
            string strValue = "";

            // Ensure the operator is lowercased for case-insensitive comparison
            string operatorLower = criterion.Operator?.ToLower() ?? string.Empty;

            switch (operatorLower)
            {
                case "startswith":
                    if (criterion.Value is string valueStart)
                    {
                        strValue = escapeSpecialChars ? EscapeSqlSpecialCharsInStringValue(valueStart, "like") : valueStart;
                        clause = $" {fieldName} LIKE '{strValue}%'";
                    }
                    else
                    {
                        throw new ApiException("Value type should be string for 'startswith' operator.");
                    }
                    break;

                case "endswith":
                    if (criterion.Value is string valueEnd)
                    {
                        strValue = escapeSpecialChars ? EscapeSqlSpecialCharsInStringValue(valueEnd, "like") : valueEnd;
                        clause = $" {fieldName} LIKE '%{strValue}'";
                    }
                    else
                    {
                        throw new ApiException("Value type should be string for 'endswith' operator.");
                    }
                    break;

                case "contains":
                    if (criterion.Value is string valueContains)
                    {
                        strValue = escapeSpecialChars ? EscapeSqlSpecialCharsInStringValue(valueContains, "like") : valueContains;
                        clause = $" {fieldName} LIKE '%{strValue}%'";
                    }
                    else
                    {
                        throw new ApiException("Value type should be string for 'contains' operator.");
                    }
                    break;

                case "isnull":
                    clause = $" {fieldName} IS NULL";
                    break;

                case "isnotnull":
                    clause = $" {fieldName} IS NOT NULL";
                    break;

                case "in":
                    if (criterion.Value is string list)
                    {
                        list = list.Trim();
                        if (!list.StartsWith("(")) list = "(" + list;
                        if (!list.EndsWith(")")) list = list + ")";
                        clause = $" {fieldName} IN {list}";
                    }
                    else
                    {
                        throw new ApiException("Value for 'in' operator should be a string.");
                    }
                    break;

                case "between":
                    if (criterion.Value is string range && range.Contains(","))
                    {
                        var splitedString = range.Split(',');
                        if (splitedString.Length == 2)
                        {
                            var from = splitedString[0].Trim();
                            var to = splitedString[1].Trim();
                            clause = $" {fieldName} BETWEEN '{from}' AND '{to}'";
                        }
                        else
                        {
                            throw new ApiException("Value for 'between' operator should contain exactly two comma-separated values.");
                        }
                    }
                    else
                    {
                        throw new ApiException("Value for 'between' operator should be a comma-separated string.");
                    }
                    break;

                default:
                    if (criterion.Value is string defaultValue)
                    {
                        strValue = escapeSpecialChars ? EscapeSqlSpecialCharsInStringValue(defaultValue, "=") : defaultValue;
                        clause = $" {fieldName} {criterion.Operator} '{strValue}'";
                    }
                    else
                    {
                        clause = $" {fieldName} {criterion.Operator} {criterion.Value}";
                    }
                    break;
            }
            return clause;
        }

        private string EscapeSqlSpecialCharsInStringValue(string value, string opr)
        {
            // Escape \ by \\ ...
            string strValue = value.Replace("\\", "\\\\");

            // Escape ' by \' ...
            strValue = strValue.Replace("'", "\\'");

            // Escape " by \" ...
            strValue = strValue.Replace("\"", "\\\"");

            // Following chars are to be escaped only if opr = like ...
            if (opr.ToLower() == "like")
            {
                // Escape % by \% ...
                strValue = strValue.Replace("%", "\\%");

                // Escape _ by \_ ...
                strValue = strValue.Replace("_", "\\_");
            }
            return strValue;
        }
    }
}
