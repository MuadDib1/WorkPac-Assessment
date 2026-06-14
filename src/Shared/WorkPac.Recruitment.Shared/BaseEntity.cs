namespace WorkPac.Recruitment.Shared;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public byte[] RowVersion { get; protected set; } = [];

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearEvents() => _domainEvents.Clear();
}

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
