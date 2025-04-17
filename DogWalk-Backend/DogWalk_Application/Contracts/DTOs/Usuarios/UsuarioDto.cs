using System;

namespace DogWalk_Application.Contracts.DTOs.Usuarios
{
    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string FotoPerfil { get; set; }
        public string Rol { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
