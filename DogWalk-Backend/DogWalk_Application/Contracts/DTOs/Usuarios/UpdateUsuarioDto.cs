namespace DogWalk_Application.Contracts.DTOs.Usuarios;

/// <summary>
/// DTO para actualizar un usuario.
/// </summary>
public class UpdateUsuarioDto
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Direccion { get; set; }
    public string Telefono { get; set; }
    public string FotoPerfil { get; set; }
}