namespace DogWalk_Application.Contracts.DTOs.Admin
{
    public class AssignRoleDto
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; } // "Admin", "Usuario", "Paseador"
    }
}