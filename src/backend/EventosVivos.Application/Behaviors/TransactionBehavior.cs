using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Common;
using Mediator;

namespace EventosVivos.Application.Behaviors;

/// <summary>
/// Commits the unit of work after the handler runs. On an optimistic concurrency conflict it
/// discards the tracked changes and retries the handler, so the operation re-reads the current
/// state. Business failures (a failed <see cref="Result"/>) are not persisted.
/// </summary>
public sealed class TransactionBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private const int MaxAttempts = 3;

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var response = await next(message, cancellationToken);

                if (response is Result { IsFailure: true })
                {
                    return response;
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                return response;
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                unitOfWork.ClearTracking();
            }
        }
    }
}
