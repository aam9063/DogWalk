using System;

namespace DogWalk_Application.Contracts.DTOs.Auth;

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime TokenExpiration { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Rol { get; set; }
}
