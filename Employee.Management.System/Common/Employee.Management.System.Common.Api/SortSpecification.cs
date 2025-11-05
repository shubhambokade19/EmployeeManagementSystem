namespace Employee.Management.System.Common.Api
{
    public class SortSpecification
    {
        private List<string?> OrderValues = new List<string?> { "ASC", "DESC" };

        public string Field { get; set; } = string.Empty;
        public string Order { get; set; } = string.Empty;

        public bool ValidateOrder(string? value)
        {
            return OrderValues.Contains(Order?.ToUpper());
        }
    }
}
