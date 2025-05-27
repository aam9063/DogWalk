using System;

namespace DogWalk_Application.Contracts.DTOs.Auth
{
    /// <summary>
    /// DTO para restablecer la contraseña de un usuario.
    /// </summary>
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
