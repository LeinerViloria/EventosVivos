namespace EventosVivos.Domain.Common;

/// <summary>
/// A business error identified by a stable code that the frontend translates via i18n.
/// Optional parameters carry the dynamic values needed to render the message.
/// </summary>
public sealed record Error(string Code, IReadOnlyDictionary<string, object?>? Parameters = null)
{
    public static readonly Error None = new(string.Empty);
}
