using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Events.CreateEvent;
using EventosVivos.Domain.Events;
using FluentValidation.TestHelper;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Events;

public class CreateEventValidatorTests
{
    // Fixed "now"; the valid command sits comfortably in the future.
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly CreateEventValidator _validator;

    public CreateEventValidatorTests()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(Now);
        _validator = new CreateEventValidator(clock);
    }

    private static CreateEventCommand AValidCommand() =>
        new("Tech Talk", "A talk about technology and more.", Guid.CreateVersion7(), 100,
            Now.AddHours(6), Now.AddHours(8), 50m, EventType.Conference);

    [Fact]
    public void Passes_for_a_valid_command()
    {
        _validator.TestValidate(AValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_when_title_length_is_out_of_range()
    {
        _validator
            .TestValidate(AValidCommand() with { Title = "Co" })
            .ShouldHaveValidationErrorFor(c => c.Title)
            .WithErrorCode("EVENT_TITLE_LENGTH");
    }

    [Fact]
    public void Fails_when_description_length_is_out_of_range()
    {
        _validator
            .TestValidate(AValidCommand() with { Description = "short" })
            .ShouldHaveValidationErrorFor(c => c.Description)
            .WithErrorCode("EVENT_DESCRIPTION_LENGTH");
    }

    [Fact]
    public void Fails_when_venue_is_empty()
    {
        _validator
            .TestValidate(AValidCommand() with { VenueId = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.VenueId)
            .WithErrorCode("EVENT_VENUE_REQUIRED");
    }

    [Fact]
    public void Fails_when_capacity_is_not_positive()
    {
        _validator
            .TestValidate(AValidCommand() with { MaxCapacity = 0 })
            .ShouldHaveValidationErrorFor(c => c.MaxCapacity)
            .WithErrorCode("EVENT_CAPACITY_POSITIVE");
    }

    [Fact]
    public void Fails_when_price_is_not_positive()
    {
        _validator
            .TestValidate(AValidCommand() with { Price = 0m })
            .ShouldHaveValidationErrorFor(c => c.Price)
            .WithErrorCode("EVENT_PRICE_POSITIVE");
    }

    [Fact]
    public void Fails_when_type_is_not_a_valid_enum()
    {
        _validator
            .TestValidate(AValidCommand() with { Type = (EventType)99 })
            .ShouldHaveValidationErrorFor(c => c.Type)
            .WithErrorCode("EVENT_TYPE_INVALID");
    }

    [Fact]
    public void Fails_when_start_is_not_in_the_future()
    {
        _validator
            .TestValidate(AValidCommand() with { StartsAt = Now.AddHours(-1) })
            .ShouldHaveValidationErrorFor(c => c.StartsAt)
            .WithErrorCode("EVENT_START_FUTURE");
    }

    [Fact]
    public void Fails_when_end_is_not_after_start()
    {
        _validator
            .TestValidate(AValidCommand() with { EndsAt = Now.AddHours(5) })
            .ShouldHaveValidationErrorFor(c => c.EndsAt)
            .WithErrorCode("EVENT_END_AFTER_START");
    }
}
