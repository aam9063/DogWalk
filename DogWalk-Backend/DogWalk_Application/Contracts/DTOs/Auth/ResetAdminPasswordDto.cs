using System;

namespace DogWalk_Application.Contracts.DTOs.Auth;

/// <summary>
/// DTO para restablecer la contraseña de un administrador.
/// </summary>
public class ResetAdminPasswordDto
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; }
    public string SecurityKey { get; set; } // ResetPasswordKey123
}
