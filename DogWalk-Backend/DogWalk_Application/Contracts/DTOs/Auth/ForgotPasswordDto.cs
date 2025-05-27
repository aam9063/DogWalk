using System;

namespace DogWalk_Application.Contracts.DTOs.Auth
{
    /// <summary>
    /// DTO para solicitar un restablecimiento de contraseña.
    /// </summary>
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }
}
