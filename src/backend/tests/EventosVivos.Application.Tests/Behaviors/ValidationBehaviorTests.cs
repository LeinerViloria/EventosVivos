using EventosVivos.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using Mediator;
using NSubstitute;

namespace EventosVivos.Application.Tests.Behaviors;

public sealed record ValidatedMessage : IMessage;

public class ValidationBehaviorTests
{
    private static ValueTask<string> Next(ValidatedMessage message, CancellationToken cancellationToken) =>
        ValueTask.FromResult("handled");

    [Fact]
    public async Task Calls_the_handler_when_there_are_no_validators()
    {
        var behavior = new ValidationBehavior<ValidatedMessage, string>([]);

        var result = await behavior.Handle(new ValidatedMessage(), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Calls_the_handler_when_validation_passes()
    {
        var validator = Substitute.For<IValidator<ValidatedMessage>>();
        validator
            .ValidateAsync(Arg.Any<ValidationContext<ValidatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var behavior = new ValidationBehavior<ValidatedMessage, string>([validator]);

        var result = await behavior.Handle(new ValidatedMessage(), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Throws_a_validation_exception_carrying_the_error_codes_when_validation_fails()
    {
        var validator = Substitute.For<IValidator<ValidatedMessage>>();
        var failure = new ValidationFailure("Title", "too short") { ErrorCode = "EVENT_TITLE_LENGTH" };
        validator
            .ValidateAsync(Arg.Any<ValidationContext<ValidatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([failure]));
        var behavior = new ValidationBehavior<ValidatedMessage, string>([validator]);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await behavior.Handle(new ValidatedMessage(), Next, CancellationToken.None));

        Assert.Contains(exception.Errors, e => e.ErrorCode == "EVENT_TITLE_LENGTH");
    }
}
