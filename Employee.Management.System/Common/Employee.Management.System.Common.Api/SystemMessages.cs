namespace Employee.Management.System.Common.Api
{
    public class SystemMessages
    {
        public const string ServiceIsNotSupported = "Service is not supported on the requested resource";
        public const string ServiceIsNotImplemented = "Service is not implemented";
        public const string AuthenticationError = "Authentication error";
        public const string NullSessionContext = "Session context is null";

        public const string UnsupportedPatchOperation = "Unsupported value for parameter \'operation\' in patch request. For supported values, refer documentation for the corresponding API.";

        public const string NoDataFound = "No data found";
        public const string ToShouldBeGreaterThanOrEqualToZero = "Parameter \'to\' should be greater than or equal zero";
        public const string FromShouldBeGreaterThanOrEqualToZero = "Parameter \'from\' should be greater than or equal zero";
        public const string ToShouldBeGreaterThanOrEqualToFrom = "Parameter \'to\' should be greater than or equal to parameter \'from\'";
        public const string FromShouldBeGreaterThanOrEqualToOneAndLessThanTo = "Parameter \'from\' should be greater than or equal to 1 and less than parameter \'to'";
        public const string InvalidValueForFilter = "Unsupported value for parameter \'filter\'. Supported values are  ALL | INACTIVE | ACTIVE";

        public const string LastModifiedDateIsRequired = "LastModifiedDate is required";
        public const string LastModifiedByShouldBeANumber = "LastModifiedBy must be a number";

        public const string LastModifiedByIsRequired = "LastModifiedBy is required";

        public const string LastModifiedViaSourceIsRequired = "LastModifiedViaSource is required";
        public const string LastModifiedViaSourceCannotExceed64Characters = "LastModifiedViaSource cannot exceed 64 characters";

        public const string NoPatchActionsFound = "No patch actions found for processing";
        public const string IdIsZero = "Id is zero";
        public const string PatchActionsKeyNotFound = "Patch actions key {0} not found";
        public const string NoDataFoundForId = "No data found for id {0}";
    }
}
