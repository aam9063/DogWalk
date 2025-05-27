using System;

namespace DogWalk_Application.Contracts.DTOs.Usuarios;

/// <summary>
/// DTO para representar un usuario administrador.
/// </summary>
public class AdminUsuarioDto
{
    public Guid Id { get; set; }
    public string Dni { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public string FotoPerfil { get; set; }
    public string Rol { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public bool Activo { get; set; }
    
    // Información adicional para administración
    public int CantidadPerros { get; set; }
    public int CantidadReservas { get; set; }
    public decimal TotalGastado { get; set; }
    public int CantidadValoracionesRealizadas { get; set; }
    
    // En caso de bloqueo/desactivación
    public bool Bloqueado { get; set; }
    public string RazonBloqueo { get; set; }
    public DateTime? FechaBloqueo { get; set; }
    
    // Acciones disponibles
    public bool PuedeEditar { get; set; } = true;
    public bool PuedeBorrar { get; set; } = true;
    public bool PuedeBloquear { get; set; } = true;
    public bool PuedeCambiarRol { get; set; } = true;
}
