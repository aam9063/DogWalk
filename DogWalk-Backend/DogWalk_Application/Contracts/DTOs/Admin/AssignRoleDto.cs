namespace DogWalk_Application.Contracts.DTOs.Admin
{
    /// <summary>
    /// DTO para asignar un rol a un usuario.
    /// </summary>
    public class AssignRoleDto
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; } // "Admin", "Usuario", "Paseador"
    }
}