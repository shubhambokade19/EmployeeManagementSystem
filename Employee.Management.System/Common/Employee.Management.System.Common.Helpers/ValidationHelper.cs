using Employee.Management.System.Common.Api;
using System.Text.RegularExpressions;

namespace Employee.Management.System.Common.Helpers
{
    public static class ValidationHelper
    {
        public static DomainValidationResult ValidateFieldLength(string? label, string? data, int maxLength, int minLength = 0)
        {
            if (minLength > 0 && data?.Length < minLength)
                return new DomainValidationResult { Success = false, Message = $"{label} should be min {minLength} characters." };

            if (maxLength > 0 && data?.Length > maxLength)
                return new DomainValidationResult { Success = false, Message = $"{label} should not exceed {maxLength} characters." };

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateNotRequiredFieldLength(string? label, string? data, int maxLength)
        {
            //Note: If data passed is null or EmptyString, this validation will be sucessful
            if (maxLength > 0 && !string.IsNullOrEmpty(data) && data.Length > maxLength)
                return new DomainValidationResult { Success = false, Message = $"{label} should not exceed {maxLength} characters." };

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateRequiredField(string? label, string? data)
        {
            if (string.IsNullOrEmpty(data))
                return new DomainValidationResult { Success = false, Message = $"{label} is required." };

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateRequiredObject(string? label, object entity)
        {
            if (entity == null)
            {
                return new DomainValidationResult { Success = false, Message = $"{label} is null.", Expectation = "" };
            }
            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateRequiredId(string? label, long id)
        {
            if (id == 0)
            {
                return new DomainValidationResult { Success = false, Message = $"{label} is required.", Expectation = "" };
            }
            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateRequiredDateTimeField(string? label, DateTime? data)
        {
            if (data == null)
                return new DomainValidationResult { Success = false, Message = $"{label} is required." };

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateMinDate(string? label, DateTime? date)
        {
            if (date == DateTime.MinValue)
                return new DomainValidationResult { Success = false, Message = $"{label} should be selected." };

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateEmail(string? email)
        {
            Regex emailPattern = new Regex(@"^\w+([-_.]?\w+)*(\+\w+)?@\w+([-_.]?\w+)*\.(\w{2,5})$");

            if (string.IsNullOrEmpty(email) || !emailPattern.IsMatch(email))
            {
                return new DomainValidationResult { Success = false, Message = $"{email} is not valid." };
            }

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateDomainName(string? label, string? data)
        {
            Regex domainPattern = new Regex(@"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-zA-Z0-9]+([\-\.]{1}[a-zA-Z0-9]+)*\.[a-z]{2,}(:[0-9]{1,})?(\/.*)?$");

            if (string.IsNullOrEmpty(data) || !domainPattern.IsMatch(data))
            {
                return new DomainValidationResult { Success = false, Message = $"{label} is not valid." };
            }

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateNonRequiredDomainName(string? label, string? data)
        {
            Regex domainPattern = new Regex(@"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-zA-Z0-9]+([\-\.]{1}[a-zA-Z0-9]+)*\.[a-z]{2,}(:[0-9]{1,})?(\/.*)?$");

            if (!string.IsNullOrEmpty(data) && !domainPattern.IsMatch(data))
            {
                return new DomainValidationResult { Success = false, Message = $"{label} is not valid." };
            }

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateRequiredFieldArray(string? label, string[] dataArray)
        {
            if (dataArray == null || dataArray.Length == 0 || dataArray.Any(string.IsNullOrEmpty))
            {
                return new DomainValidationResult { Success = false, Message = $"{label} is required." };
            }

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateNonRequiredEmail(string? email)
        {
            Regex emailPattern = new Regex(@"^\w+([-_.]?\w+)*(\+\w+)?@\w+([-_.]?\w+)*\.\w{2,3}$");

            if (!string.IsNullOrEmpty(email) && !emailPattern.IsMatch(email))
            {
                return new DomainValidationResult { Success = false, Message = $"{email} is not valid." };
            }

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateNonNullableBoolField(string? label, bool? data)
        {
            if (data == null)
            {
                return new DomainValidationResult { Success = false, Message = $"{label} is required." };
            }

            return new DomainValidationResult { Success = true };
        }
        public static DomainValidationResult ValidateRequiredMobileNumber(string? label, string? data)
        {
            Regex mobileNumberPattern = new Regex(@"^[6-9]\d{9}$");
            if (string.IsNullOrEmpty(data))
                return new DomainValidationResult { Success = false, Message = $"{label} is required." };

            if (!mobileNumberPattern.IsMatch(data))
                return new DomainValidationResult { Success = false, Message = $"{label} is not valid." };

            return new DomainValidationResult { Success = true };
        }
    }
}
