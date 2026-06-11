using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Behaviors;
using EventosVivos.Domain.Common;
using Mediator;
using NSubstitute;

namespace EventosVivos.Application.Tests.Behaviors;

public sealed record TransactionalMessage : IMessage;

public class TransactionBehaviorTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Commits_when_the_handler_succeeds()
    {
        var behavior = new TransactionBehavior<TransactionalMessage, Result>(_unitOfWork);
        var response = Result.Success();

        var result = await behavior.Handle(
            new TransactionalMessage(),
            (_, _) => ValueTask.FromResult(response),
            CancellationToken.None);

        Assert.Same(response, result);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_commit_when_the_result_is_a_business_failure()
    {
        var behavior = new TransactionBehavior<TransactionalMessage, Result>(_unitOfWork);

        var result = await behavior.Handle(
            new TransactionalMessage(),
            (_, _) => ValueTask.FromResult(Result.Failure(new Error("VENUE_NOT_FOUND"))),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Clears_tracking_and_retries_on_a_concurrency_conflict()
    {
        var behavior = new TransactionBehavior<TransactionalMessage, Result>(_unitOfWork);
        var attempts = 0;

        var result = await behavior.Handle(
            new TransactionalMessage(),
            (_, _) =>
            {
                attempts++;
                return attempts == 1
                    ? throw new ConcurrencyConflictException(new InvalidOperationException("conflict"))
                    : ValueTask.FromResult(Result.Success());
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, attempts);
        _unitOfWork.Received(1).ClearTracking();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
