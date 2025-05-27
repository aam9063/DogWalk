using System;

namespace DogWalk_Application.Contracts.DTOs.Auth
{
    /// <summary>
    /// DTO para iniciar sesión.
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
