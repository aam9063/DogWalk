using System;

namespace DogWalk_Application.Contracts.DTOs.Auth
{
    /// <summary>
    /// DTO para cambiar la contraseña de un usuario.
    /// </summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
