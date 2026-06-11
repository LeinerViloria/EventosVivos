namespace EventosVivos.Infrastructure.Persistence.Outbox;

/// <summary>
/// A pending integration event persisted in the same transaction as its business change, so it is
/// never published unless the change committed, and survives a crash to be retried (Outbox pattern).
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        // Required by EF Core.
    }

    private OutboxMessage(Guid id, string type, string payload, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = null!;

    public string Payload { get; private set; } = null!;

    public DateTime OccurredOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public int RetryCount { get; private set; }

    public static OutboxMessage Create(string type, string payload, DateTime occurredOnUtc) =>
        new(Guid.CreateVersion7(), type, payload, occurredOnUtc);

    public void MarkProcessed(DateTime now) => ProcessedOnUtc = now;

    public void RecordFailure() => RetryCount++;
}
