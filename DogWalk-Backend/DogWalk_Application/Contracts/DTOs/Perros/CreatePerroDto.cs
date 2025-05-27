namespace DogWalk_Application.Contracts.DTOs.Perros
{
    /// <summary>
    /// DTO para crear un nuevo perro.
    /// </summary>
    public class CreatePerroDto
    {
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public int Edad { get; set; }
        public string GpsUbicacion { get; set; }
    }
}