using EventosVivos.Infrastructure.Messaging;

namespace EventosVivos.Api.Endpoints.Events;

public sealed class EventStreamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/events/stream", async (
            HttpContext httpContext,
            EventStreamBroadcaster broadcaster,
            CancellationToken cancellationToken) =>
        {
            // Subscribe before flushing the headers so no event is missed between connect and listen.
            var (id, reader) = broadcaster.Subscribe();

            httpContext.Response.Headers.ContentType = "text/event-stream";
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers["X-Accel-Buffering"] = "no";
            await httpContext.Response.Body.FlushAsync(cancellationToken);

            try
            {
                await foreach (var data in reader.ReadAllAsync(cancellationToken))
                {
                    await httpContext.Response.WriteAsync($"data: {data}\n\n", cancellationToken);
                    await httpContext.Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // The client disconnected; fall through to clean up the subscription.
            }
            finally
            {
                broadcaster.Unsubscribe(id);
            }
        })
        .WithTags("Events")
        .RequireAuthorization();
    }
}
