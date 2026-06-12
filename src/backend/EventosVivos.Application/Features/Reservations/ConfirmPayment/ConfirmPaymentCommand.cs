using EventosVivos.Domain.Common;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.ConfirmPayment;

public sealed record ConfirmPaymentCommand(Guid ReservationId) : IRequest<Result<ConfirmPaymentResponse>>;
