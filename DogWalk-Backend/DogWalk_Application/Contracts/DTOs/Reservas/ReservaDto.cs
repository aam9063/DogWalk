using System;

namespace DogWalk_Application.Contracts.DTOs.Reservas;

public class ReservaDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string NombreUsuario { get; set; }
    public Guid PaseadorId { get; set; }
    public string NombrePaseador { get; set; }
    public Guid PerroId { get; set; }
    public string NombrePerro { get; set; }
    public Guid ServicioId { get; set; }
    public string NombreServicio { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime FechaServicio { get; set; }
    public string Estado { get; set; }
    public decimal Precio { get; set; }
    public string DireccionRecogida { get; set; }
    public string DireccionEntrega { get; set; }
}
