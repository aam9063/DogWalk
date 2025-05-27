namespace DogWalk_Application.Contracts.DTOs.Admin
{
    /// <summary>
    /// DTO para crear un nuevo administrador.
    /// </summary>
    public class CreateAdminDto
    {
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Telefono { get; set; }
    }
}