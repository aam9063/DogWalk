using System;

namespace DogWalk_Application.Contracts.DTOs.Admin;

public class AdminUserDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Telefono { get; set; }
}
