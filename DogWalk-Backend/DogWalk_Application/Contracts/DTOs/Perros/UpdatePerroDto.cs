namespace DogWalk_Application.Contracts.DTOs.Perros
{
    /// <summary>
    /// DTO para actualizar un perro.
    /// </summary>
    public class UpdatePerroDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public int Edad { get; set; }
        public string GpsUbicacion { get; set; }
    }
}