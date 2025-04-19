using System;
using DogWalk_Application.Contracts.DTOs.Paseadores;
using DogWalk_Application.Contracts.DTOs.Perros;
using DogWalk_Application.Contracts.DTOs.Servicios;
using DogWalk_Application.Contracts.DTOs.Usuarios;

namespace DogWalk_Application.Contracts.DTOs.Reservas;

public class ReservaDetailsDto
{
    public Guid Id { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime FechaServicio { get; set; }
    public string Estado { get; set; }
    public decimal Precio { get; set; }
    public string DireccionRecogida { get; set; }
    public string DireccionEntrega { get; set; }
    public string Notas { get; set; }
    public bool PuedeEditar { get; set; }
    public bool PuedeCancelar { get; set; }
    public bool PuedeCompletar { get; set; }
    public bool PuedeValorar { get; set; }
    
    // Información relacionada
    public UsuarioDto Usuario { get; set; }
    public PaseadorDto Paseador { get; set; }
    public PerroDto Perro { get; set; }
    public ServicioDto Servicio { get; set; }
    
    // Valoración (si existe)
    public ValoracionDto Valoracion { get; set; }
}
