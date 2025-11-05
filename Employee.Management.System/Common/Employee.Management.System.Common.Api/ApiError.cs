namespace Employee.Management.System.Common.Api
{
    public class ApiError
    {
        public static readonly string Information = "I";
        public static readonly string Warning = "W";
        public static readonly string Error = "E";

        public string Type { get; set; } = Error;
        public string? Message { get; set; }
        public string? Source { get; set; }
    }
}
