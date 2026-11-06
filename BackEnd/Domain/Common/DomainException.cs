namespace OnlineShop.Domain.Common;

/// <summary>One form field that caused a failure. Field is the property name in any casing.</summary>
public sealed record DomainFieldError(string Field, string Message);

/// <summary>
/// A failure that can be explained to the user. <see cref="Exception.Message"/> is that explanation,
/// in Persian, and goes to the client as it is. Anything that is not a DomainException is treated as
/// a server fault and its message is never shown to visitors.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    /// <summary>The raw technical cause, for logs and admins. Never sent to visitors.</summary>
    public string? TechnicalDetail { get; init; }

    public IReadOnlyList<DomainFieldError>? FieldErrors { get; init; }

    public DomainException(string message, string code = "DOMAIN_ERROR")
        : base(message)
    {
        Code = code;
    }

    public DomainException(string message, Exception inner, string code = "DOMAIN_ERROR")
        : base(message, inner)
    {
        Code = code;
    }
}

/// <summary>The requested record does not exist (HTTP 404).</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message, string code = "NOT_FOUND")
        : base(message, code)
    {
    }
}

/// <summary>The request clashes with the current data: a duplicate, a record in use, a concurrent edit (HTTP 409).</summary>
public class ConflictException : DomainException
{
    public ConflictException(string message, string code = "CONFLICT")
        : base(message, code)
    {
    }

    public ConflictException(string message, Exception inner, string code = "CONFLICT")
        : base(message, inner, code)
    {
    }
}

/// <summary>Something the server relies on is down or misconfigured: database, payment gateway (HTTP 503).</summary>
public class DependencyFailureException : DomainException
{
    public DependencyFailureException(string message, string code = "SERVICE_UNAVAILABLE")
        : base(message, code)
    {
    }

    public DependencyFailureException(string message, Exception inner, string code = "SERVICE_UNAVAILABLE")
        : base(message, inner, code)
    {
    }
}

/// <summary>An uploaded file was refused or could not be stored. The message says why.</summary>
public class FileUploadException : DomainException
{
    public FileUploadException(string message)
        : base(message, "UPLOAD_FAILED")
    {
    }

    public FileUploadException(string message, Exception inner)
        : base(message, inner, "UPLOAD_FAILED")
    {
    }
}
