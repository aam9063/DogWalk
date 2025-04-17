using System;

namespace DogWalk_Application.Contracts.DTOs.Usuarios
{
    public class UsuarioProfileDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string FotoPerfil { get; set; }
        public int CantidadPerros { get; set; }
        public int CantidadReservas { get; set; }
    }
}
