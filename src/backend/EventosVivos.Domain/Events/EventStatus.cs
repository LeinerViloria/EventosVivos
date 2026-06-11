namespace EventosVivos.Domain.Events;

public enum EventStatus : byte
{
    Active = 1,
    Cancelled = 2,
    Completed = 3,
}
