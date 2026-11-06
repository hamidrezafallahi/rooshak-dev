namespace OnlineShop.Domain.Common;

/// <summary>
/// One string value that is longer than its column allows.
/// Field is the camelCase property name, which matches the admin form field name.
/// Label is the name people know the field by.
/// </summary>
public sealed record FieldLengthViolation(string Field, int MaxLength, int ActualLength, string Label)
{
    public string Message => $"«{Label}» حداکثر {MaxLength} نویسه می‌تواند باشد، اما {ActualLength} نویسه وارد شده است.";
}

/// <summary>
/// Raised before SaveChanges when string values exceed their configured max length,
/// so the API can name the offending fields instead of failing in the database.
/// </summary>
public class FieldLengthExceededException : DomainException
{
    public IReadOnlyList<FieldLengthViolation> Violations { get; }

    public FieldLengthExceededException(IReadOnlyList<FieldLengthViolation> violations)
        : base(string.Join(" ", violations.Select(v => v.Message)), "FIELD_TOO_LONG")
    {
        Violations = violations;
        FieldErrors = violations.Select(v => new DomainFieldError(v.Field, v.Message)).ToList();
    }
}
