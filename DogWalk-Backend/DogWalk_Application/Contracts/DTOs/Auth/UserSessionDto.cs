using System;
using DogWalk_Domain.Common.Enums;

namespace DogWalk_Application.Contracts.DTOs.Auth;

public record UserSessionDto
{
    public Guid Id { get; init; }
    public string Dni { get; init; }
    public string Nombre { get; init; }
    public string Apellido { get; init; }
    public string Email { get; init; }
    public string Direccion { get; init; }
    public string Telefono { get; init; }
    public string FotoPerfil { get; init; }
    public RolUsuario Rol { get; init; }
}