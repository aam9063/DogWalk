using System;

namespace DogWalk_Application.Contracts.DTOs.Paseadores;

/// <summary>
/// DTO para representar una valoración de un paseador.
/// </summary>
public class PaseadorValoracionDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string NombreUsuario { get; set; }
    public int Puntuacion { get; set; }
    public string Comentario { get; set; }
    public DateTime Fecha { get; set; }
}
