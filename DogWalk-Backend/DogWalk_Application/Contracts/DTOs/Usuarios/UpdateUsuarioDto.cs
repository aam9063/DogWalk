using System;

namespace DogWalk_Application.Contracts.DTOs.Usuarios
{
    public class UpdateUsuarioDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
    }
}
