namespace Employee.Management.System.Common.Api
{
    public class ApiUnauthorizedAccessException : ApiException
    {
        public ApiUnauthorizedAccessException() : base()
        {
            ErrorList = new List<ApiError>();
        }
        public ApiUnauthorizedAccessException(string arg0) : base(arg0)
        {
        }

        public ApiUnauthorizedAccessException(string arg0, string messageKey)
            : base(arg0, messageKey)
        {
        }

        public ApiUnauthorizedAccessException(string arg0, Exception arg1)
            : base(arg0, arg1)
        {
        }

        public ApiUnauthorizedAccessException(string arg0, Exception arg1, string messageKey)
            : base(arg0, arg1, messageKey)
        {
        }

        public ApiUnauthorizedAccessException(List<DomainValidationResult> validationErrors) : base(validationErrors)
        {
        }
    }
}
