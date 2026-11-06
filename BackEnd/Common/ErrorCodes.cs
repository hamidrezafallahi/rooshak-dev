namespace Common
{
    /// <summary>
    /// Machine-readable reason sent as <c>errorCode</c> next to the readable <c>error</c> text.
    /// The text is what people read; the code is for the frontend to branch on.
    /// </summary>
    public static class ErrorCodes
    {
        public const string BadRequest = "BAD_REQUEST";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string NotFound = "NOT_FOUND";
        public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
        public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";
        public const string RequestTimeout = "REQUEST_TIMEOUT";
        public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
        public const string RateLimited = "RATE_LIMITED";
        public const string IpBlocked = "IP_BLOCKED";

        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string DomainRule = "DOMAIN_ERROR";
        public const string Conflict = "CONFLICT";
        public const string FieldTooLong = "FIELD_TOO_LONG";
        public const string DuplicateValue = "DUPLICATE_VALUE";
        public const string ReferenceInUse = "REFERENCE_IN_USE";
        public const string ReferenceMissing = "REFERENCE_MISSING";
        public const string RequiredValueMissing = "REQUIRED_VALUE_MISSING";
        public const string InvalidData = "INVALID_DATA";
        public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";

        public const string DatabaseUnavailable = "DATABASE_UNAVAILABLE";
        public const string DatabaseTimeout = "DATABASE_TIMEOUT";
        public const string DatabaseBusy = "DATABASE_BUSY";
        public const string DatabaseResources = "DATABASE_RESOURCES";
        public const string DatabaseSchemaMismatch = "DATABASE_SCHEMA_MISMATCH";
        public const string DatabaseError = "DATABASE_ERROR";

        public const string UploadFailed = "UPLOAD_FAILED";
        public const string StorageError = "STORAGE_ERROR";
        public const string PaymentGatewayError = "PAYMENT_GATEWAY_ERROR";
        public const string ExternalServiceError = "EXTERNAL_SERVICE_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string InternalError = "INTERNAL_ERROR";
    }

    /// <summary>Texts used by more than one layer. Everything else lives where the failure happens.</summary>
    public static class ErrorMessages
    {
        public const string LoginRequired = "برای انجام این کار ابتدا وارد حساب کاربری خود شوید.";
        public const string SessionExpired = "نشست کاربری شما منقضی شده است؛ لطفاً دوباره وارد شوید.";
        public const string SessionInvalid = "اطلاعات ورود شما معتبر نیست؛ لطفاً دوباره وارد شوید.";
        public const string Forbidden = "حساب کاربری شما اجازهٔ انجام این کار را ندارد.";
    }
}
