namespace Employee.Management.System.Common.Api
{
    public class SearchCriterion
    {
        public static readonly string[] AllowedOperators = new string[]
        {
            "=", "<>", ">", ">=", "<", "<=",
            "startswith", "endswith", "contains", "isnull", "isnotnull", "in", "between", "notstartswith", "containsorcontains"
        };

        public const string EqualTo = "=";
        public const string NotEqualTo = "<>";
        public const string GreaterThan = ">";
        public const string GreatrThanOrEqualTo = ">=";
        public const string LessThan = "<";
        public const string LessThanOrEqualTo = "<=";
        public const string StartsWith = "startswith";
        public const string EndsWith = "endswith";
        public const string Contains = "contains";
        public const string IsNull = "isnull";
        public const string IsNotNull = "isnotnull";
        public const string In = "in";
        public const string Between = "between";
        public const string NotStartsWith = "notstartswith";
        public const string ContainsOrContains = "containsorcontains";

        public string? Field { get; set; } = null;
        public object? Value { get; set; } = null;
        private string operatorString = "=";

        public string Operator
        {
            get { return operatorString.ToLower(); }
            set
            {
                if (Array.IndexOf(AllowedOperators, value.ToLower()) > -1)
                    operatorString = value.ToLower();
                else
                    throw new ApiException($"{value} should be one of {AllowedOperators.ToString()}");
            }
        }
    }
}
