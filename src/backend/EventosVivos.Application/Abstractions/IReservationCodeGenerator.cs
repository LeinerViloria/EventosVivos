namespace EventosVivos.Application.Abstractions;

/// <summary>Generates a reservation confirmation code in the format EV-{6 digits}.</summary>
public interface IReservationCodeGenerator
{
    string Generate();
}
