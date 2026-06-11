using EventosVivos.Application.Features.Reservations.CreateReservation;
using FluentValidation.TestHelper;

namespace EventosVivos.Application.Tests.Features.Reservations;

public class CreateReservationValidatorTests
{
    private readonly CreateReservationValidator _validator = new();

    private static CreateReservationCommand ACommand() =>
        new(Guid.CreateVersion7(), "Ana Compradora", "ana@example.com", 2);

    [Fact]
    public void Passes_for_a_valid_command()
    {
        _validator.TestValidate(ACommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_when_quantity_is_not_positive()
    {
        _validator
            .TestValidate(ACommand() with { Quantity = 0 })
            .ShouldHaveValidationErrorFor(c => c.Quantity)
            .WithErrorCode("RESERVATION_QUANTITY_POSITIVE");
    }

    [Fact]
    public void Fails_when_the_buyer_email_is_invalid()
    {
        _validator
            .TestValidate(ACommand() with { BuyerEmail = "not-an-email" })
            .ShouldHaveValidationErrorFor(c => c.BuyerEmail)
            .WithErrorCode("RESERVATION_BUYER_EMAIL_INVALID");
    }

    [Fact]
    public void Fails_when_the_buyer_name_is_empty()
    {
        _validator
            .TestValidate(ACommand() with { BuyerName = "" })
            .ShouldHaveValidationErrorFor(c => c.BuyerName)
            .WithErrorCode("RESERVATION_BUYER_NAME_REQUIRED");
    }
}
