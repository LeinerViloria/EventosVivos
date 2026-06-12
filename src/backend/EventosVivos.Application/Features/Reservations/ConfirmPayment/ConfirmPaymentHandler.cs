using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Reservations;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.ConfirmPayment;

public sealed class ConfirmPaymentHandler(
    IReservationRepository reservations,
    IReservationCodeGenerator codeGenerator,
    IClock clock) : IRequestHandler<ConfirmPaymentCommand, Result<ConfirmPaymentResponse>>
{
    private const int MaxCodeAttempts = 10;

    public async ValueTask<Result<ConfirmPaymentResponse>> Handle(
        ConfirmPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
        {
            return Result.Failure<ConfirmPaymentResponse>(ReservationErrors.NotFound);
        }

        var code = await GenerateUniqueCodeAsync(cancellationToken);

        var result = reservation.Confirm(code, clock.UtcNow);
        if (result.IsFailure)
        {
            return Result.Failure<ConfirmPaymentResponse>(result.Error);
        }

        return Result.Success(new ConfirmPaymentResponse(code));
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeAttempts; attempt++)
        {
            var code = codeGenerator.Generate();
            if (!await reservations.CodeExistsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        // Extremely unlikely; the unique index is the final guard if this ever collides.
        return codeGenerator.Generate();
    }
}
