namespace EventosVivos.Application.Abstractions;

/// <summary>
/// A page of results plus the total number of matches, so the client can render server-side
/// pagination. The database resolves the count and the page; collections are never paged in memory.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
