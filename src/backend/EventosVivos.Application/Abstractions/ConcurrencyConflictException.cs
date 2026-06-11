namespace EventosVivos.Application.Abstractions;

/// <summary>
/// Raised when an optimistic concurrency conflict is detected while saving. Lets the
/// application layer handle retries without depending on the persistence technology.
/// </summary>
public sealed class ConcurrencyConflictException(Exception innerException)
    : Exception("A concurrency conflict occurred while saving changes.", innerException);
