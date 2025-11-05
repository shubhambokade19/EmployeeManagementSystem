namespace Employee.Management.System.Common.Api
{
    public class DomainValidationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; } = null;
        public string? Expectation { get; set; } = null;
    }
}
