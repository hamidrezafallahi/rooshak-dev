using System.Text.Json.Serialization;

namespace Common
{
    public class ServiceResponse
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }

        /// <summary>See <see cref="ErrorCodes"/>. Left out of the JSON when there is no code.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ErrorCode { get; set; }

        /// <summary>Support reference of a failed request; matches the server log.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string TraceId { get; set; }

        /// <summary>The caller is not signed in: controllers answer these with HTTP 401.</summary>
        [JsonIgnore]
        public bool IsUnauthorized => !IsSuccess && ErrorCode == ErrorCodes.Unauthorized;

        public static ServiceResponse Ok()
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Error = null
            };
        }

        public static ServiceResponse Failed(string error, string errorCode = null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Error = error,
                ErrorCode = errorCode
            };
        }

        public static ServiceResponse Unauthorized(string error = ErrorMessages.LoginRequired)
        {
            return Failed(error, ErrorCodes.Unauthorized);
        }
    }

    public class ServiceResult<T> : ServiceResponse where T : class
    {
        public T Data { get; set; }

        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data=data,
                Error = null
            };
        }

        public static new ServiceResult<T> Failed(string error, string errorCode = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Error = error,
                ErrorCode = errorCode
            };
        }

        public static new ServiceResult<T> Unauthorized(string error = ErrorMessages.LoginRequired)
        {
            return Failed(error, ErrorCodes.Unauthorized);
        }

    }
}
