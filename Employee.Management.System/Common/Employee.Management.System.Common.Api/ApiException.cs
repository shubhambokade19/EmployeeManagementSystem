using System.Net;

namespace Employee.Management.System.Common.Api
{
    [Serializable]
    public class ApiException : Exception
    {
        public string? DebugInformation { get; set; } = null;
        public string? MessageKey { get; set; } = null;
        public List<ApiError> ErrorList { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public ApiException() : base()
        {
            ErrorList = new List<ApiError>();
            StatusCode = HttpStatusCode.BadRequest;
        }

        public ApiException(string arg0) : base(arg0)
        {
            ErrorList = new List<ApiError>();
            StatusCode = HttpStatusCode.BadRequest;
        }

        public ApiException(string arg0, string messageKey)
            : base(arg0)
        {
            MessageKey = messageKey;
            ErrorList = new List<ApiError>
            {
                new ApiError { Message = arg0 }
            };
            StatusCode = HttpStatusCode.BadRequest;
        }

        public ApiException(string arg0, Exception arg1)
            : base(arg0, arg1)
        {
            ErrorList = new List<ApiError>
            {
                new ApiError { Message = arg0 }
            };
            StatusCode = HttpStatusCode.BadRequest;
        }

        public ApiException(string arg0, Exception arg1, string messageKey)
            : base(arg0, arg1)
        {
            MessageKey = messageKey;
            ErrorList = new List<ApiError>
            {
                new ApiError { Message = arg0 }
            };
            StatusCode = HttpStatusCode.BadRequest;
        }

        public ApiException(List<DomainValidationResult> validationErrors) : base()
        {
            ErrorList = new List<ApiError>();
            StatusCode = HttpStatusCode.BadRequest;
            foreach (var error in validationErrors)
            {
                ErrorList.Add(new ApiError
                {
                    Message = error.Message
                });
            }
        }

        public ApiException(List<DomainValidationResult> validationErrors, HttpStatusCode statusCode) : base()
        {
            ErrorList = new List<ApiError>();
            StatusCode = statusCode;
            foreach (var error in validationErrors)
            {
                ErrorList.Add(new ApiError
                {
                    Message = error.Message
                });
            }
        }

        public ApiException(string arg0, HttpStatusCode statusCode) : base(arg0)
        {
            StatusCode = statusCode;
            ErrorList = new List<ApiError>
            {
                new ApiError { Message = arg0 }
            };
        }
    }
}
