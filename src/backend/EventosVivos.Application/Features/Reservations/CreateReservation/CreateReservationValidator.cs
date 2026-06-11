using FluentValidation;

namespace EventosVivos.Application.Features.Reservations.CreateReservation;

public sealed class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(c => c.EventId).NotEmpty().WithErrorCode("RESERVATION_EVENT_REQUIRED");
        RuleFor(c => c.BuyerName)
            .NotEmpty()
            .WithErrorCode("RESERVATION_BUYER_NAME_REQUIRED")
            .MaximumLength(120)
            .WithErrorCode("RESERVATION_BUYER_NAME_REQUIRED");
        RuleFor(c => c.BuyerEmail)
            .NotEmpty()
            .WithErrorCode("RESERVATION_BUYER_EMAIL_INVALID")
            .EmailAddress()
            .WithErrorCode("RESERVATION_BUYER_EMAIL_INVALID");
        RuleFor(c => c.Quantity).GreaterThan(0).WithErrorCode("RESERVATION_QUANTITY_POSITIVE");
    }
}
