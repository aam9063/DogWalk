using DogWalk_Domain.Entities;

namespace DogWalk_Application.Contracts.Services;

/// <summary>
/// Interfaz para el servicio de JWT.
/// </summary>
public interface IJwtService
{
    string GenerateToken(Usuario usuario);
}
