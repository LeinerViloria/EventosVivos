namespace EventosVivos.Api.Endpoints;

/// <summary>A single Minimal API endpoint (one per vertical slice).</summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
