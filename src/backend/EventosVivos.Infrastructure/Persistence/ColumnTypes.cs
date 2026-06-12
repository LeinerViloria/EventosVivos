namespace EventosVivos.Infrastructure.Persistence;

/// <summary>Postgres column type names reused across the Fluent API configurations.</summary>
internal static class ColumnTypes
{
    /// <summary>UTC instant; the backend persists every timestamp in UTC.</summary>
    public const string TimestampWithTimeZone = "timestamp with time zone";

    /// <summary>Backing store for byte-based enums.</summary>
    public const string SmallInt = "smallint";
}
