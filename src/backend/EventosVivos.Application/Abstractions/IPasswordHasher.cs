namespace EventosVivos.Application.Abstractions;

/// <summary>Hashes and verifies passwords. Implemented in the infrastructure (BCrypt).</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
