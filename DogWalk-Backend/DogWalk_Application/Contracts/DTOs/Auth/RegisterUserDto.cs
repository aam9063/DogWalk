using System;

namespace DogWalk_Application.Contracts.DTOs.Auth
{
    public class RegisterUserDto
    {
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Telefono { get; set; }
    }
}
