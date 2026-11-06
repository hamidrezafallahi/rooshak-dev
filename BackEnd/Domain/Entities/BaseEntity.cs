using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities;

/// <summary>
/// Base class for all domain entities.
/// Provides soft-delete, audit trails, and domain event collection.
/// 
/// Design decisions:
/// - Private setters enforce encapsulation (domain behavior drives state changes)
/// - Domain events collected and dispatched by infrastructure before SaveChanges
/// - Soft delete preserves referential integrity and enables auditing
/// - Audit fields are private set to prevent accidental modification
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public int Id { get; protected set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted { get; private set; }
    public bool IsActive { get; private set; } = true;

    public int? CreatedBy { get; private set; }
    public int? UpdatedBy { get; private set; }
    public int? DeletedBy { get; private set; }

    // ─── Domain Events (Core DDD pattern) ────────────────────────────────

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // ─── Audit Methods ────────────────────────────────────────────────────

    protected void MarkCreated(int userId)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
        IsActive = true;
    }

    protected void MarkUpdated(int userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    protected void MarkDeleted(int userId)
    {
        if (IsDeleted)
            throw new DomainException("این رکورد قبلاً حذف شده است.");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
        AddDomainEvent(new EntityDeletedEvent(GetType().Name, Id));
    }

    // ─── Public Behavior Methods ─────────────────────────────────────────

    public void SetActive(bool isActive, int userId)
    {
        IsActive = isActive;
        MarkUpdated(userId);
    }

    public void Delete(int currentUserId)
    {
        MarkDeleted(currentUserId);
    }
}

/// <summary>
/// Domain event raised when an entity is deleted.
/// Can be handled to perform cleanup, re-indexing, or notification.
/// </summary>
public sealed record EntityDeletedEvent(string EntityType, int EntityId) : DomainEvent;

