using System;

namespace DogWalk_Application.Contracts.DTOs.Admin;

/// <summary>
/// DTO para representar un usuario administrador.
/// </summary>
public class AdminUserDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Telefono { get; set; }
}
