using DogWalk_Application.Contracts.DTOs.Auth;
using DogWalk_Application.Contracts.DTOs.Reservas;

namespace DogWalk_Application.Contracts.DTOs.Paseadores
{
    /// <summary>
    /// DTO para representar los detalles de un paseador.
    /// </summary>
    public class PaseadorDetailsDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string FotoPerfil { get; set; }
        public decimal ValoracionGeneral { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public List<ServicioPrecioDto> Servicios { get; set; } = new List<ServicioPrecioDto>();
        public List<DisponibilidadDto> Disponibilidad { get; set; } = new List<DisponibilidadDto>();
        public List<ValoracionDto> Valoraciones { get; set; } = new List<ValoracionDto>();
        public List<ReservaDto> ReservasProximas { get; set; } = new List<ReservaDto>();
    }

    /// <summary>
    /// DTO para representar la disponibilidad de un paseador.
    /// </summary>
    public class DisponibilidadDto
    {
        public Guid Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; } // "Disponible" o "Reservado"
    }

    /// <summary>
    /// DTO para representar una valoración de un paseador.
    /// </summary>
    public class ValoracionDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string FotoUsuario { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
        public DateTime Fecha { get; set; }
    }
}