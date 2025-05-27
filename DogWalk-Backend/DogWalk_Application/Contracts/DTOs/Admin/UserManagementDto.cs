using System;
using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Admin;

/// <summary>
/// DTO para la gestión de usuarios.
/// </summary>
public class UserManagementDto
{
    public Guid Id { get; set; }
    public string Dni { get; set; }
    public string NombreCompleto { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Rol { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
    
    // Estadísticas básicas
    public int TotalPerros { get; set; }
    public int TotalReservas { get; set; }
    public decimal TotalFacturado { get; set; }
    
    // Para paseadores
    public decimal? ValoracionPromedio { get; set; }
    public int? TotalValoraciones { get; set; }
    
    // Estado de la cuenta
    public bool Bloqueado { get; set; }
    public DateTime? FechaUltimoAcceso { get; set; }
    
    // Flags para controlar acciones disponibles en la UI
    public List<string> AccionesDisponibles { get; set; } = new List<string>();
}
