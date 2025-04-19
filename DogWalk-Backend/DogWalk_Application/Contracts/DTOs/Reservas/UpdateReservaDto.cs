using System;

namespace DogWalk_Application.Contracts.DTOs.Reservas;

public class UpdateReservaDto
{
    public Guid Id { get; set; }
    public string Estado { get; set; }
    public DateTime? FechaServicio { get; set; }
    public string DireccionRecogida { get; set; }
    public string DireccionEntrega { get; set; }
    public string Notas { get; set; }
}
