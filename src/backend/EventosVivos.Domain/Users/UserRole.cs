namespace EventosVivos.Domain.Users;

/// <summary>Application roles. The contract travels as the number (enum : byte).</summary>
public enum UserRole : byte
{
    Admin = 1,
    User = 2,
}
