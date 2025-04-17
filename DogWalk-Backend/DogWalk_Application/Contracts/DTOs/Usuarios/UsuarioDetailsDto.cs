using System;

using DogWalk_Application.Contracts.DTOs.Perros;
using DogWalk_Application.Contracts.DTOs.Reservas;

namespace DogWalk_Application.Contracts.DTOs.Usuarios
{
    public class UsuarioDetailsDto
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
        public List<PerroDto> Perros { get; set; } = new List<PerroDto>();
        public List<ReservaDto> Reservas { get; set; } = new List<ReservaDto>();
    }
}
