using EventosVivos.Application.Abstractions;
using FluentValidation;

namespace EventosVivos.Application.Features.Events.CreateEvent;

/// <summary>
/// Input validation for RF-01 (structure and basic temporal rules). Business invariants
/// (RN01, RN02, RN03) live in the domain and the handler.
/// </summary>
public sealed class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator(IClock clock)
    {
        RuleFor(c => c.Title).NotEmpty().Length(5, 100).WithErrorCode("EVENT_TITLE_LENGTH");
        RuleFor(c => c.Description).NotEmpty().Length(10, 500).WithErrorCode("EVENT_DESCRIPTION_LENGTH");
        RuleFor(c => c.VenueId).NotEmpty().WithErrorCode("EVENT_VENUE_REQUIRED");
        RuleFor(c => c.MaxCapacity).GreaterThan(0).WithErrorCode("EVENT_CAPACITY_POSITIVE");
        RuleFor(c => c.Price).GreaterThan(0m).WithErrorCode("EVENT_PRICE_POSITIVE");
        RuleFor(c => c.Type).IsInEnum().WithErrorCode("EVENT_TYPE_INVALID");
        RuleFor(c => c.StartsAt).GreaterThan(_ => clock.UtcNow).WithErrorCode("EVENT_START_FUTURE");
        RuleFor(c => c.EndsAt).GreaterThan(c => c.StartsAt).WithErrorCode("EVENT_END_AFTER_START");
    }
}
