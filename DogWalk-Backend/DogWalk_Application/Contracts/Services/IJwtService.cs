using DogWalk_Domain.Entities;

namespace DogWalk_Application.Contracts.Services;

public interface IJwtService
{
    string GenerateToken(Usuario usuario);
}
