using System;

namespace DogWalk_Application.Contracts.DTOs.Reservas;

/// <summary>
/// DTO para crear una nueva reserva.
/// </summary>
public class CreateReservaDto
{
    public Guid PaseadorId { get; set; }
    public Guid PerroId { get; set; }
    public Guid ServicioId { get; set; }
    public DateTime FechaServicio { get; set; }
    public string DireccionRecogida { get; set; }
    public string DireccionEntrega { get; set; }
    public string Notas { get; set; }
}
